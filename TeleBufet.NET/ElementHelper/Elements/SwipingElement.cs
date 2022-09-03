using System.Collections.Immutable;
using System.Numerics;
using TeleBufet.NET.CacheManager.CustomCacheHelper.ReservationsCache;

namespace TeleBufet.NET.ElementHelper.Elements
{
    internal sealed class SwipingElement : UpdateElement<TicketHolder[], Grid>
    {
        private static readonly ImmutableArray<SwipeGestureRecognizer> directions = 
           ImmutableArray.Create
            (
               new SwipeGestureRecognizer() { Direction = SwipeDirection.Left, Threshold = 30 },
               new SwipeGestureRecognizer() { Direction = SwipeDirection.Right, Threshold = 30 }
            );

        private static readonly Vector2 ticketDefaultPosition = new(0, 25);
        private static readonly Vector2 ticketDestinationPosition = new(625, 25);

        private Color baseColor = Color.FromRgb(241, 226, 126);
        private Vector2 ticketPosition;
        private SwipeDirection defaultSwipeDirection;

        public SwipeDirection CurrentSwipeDirection { get; private set; }
        public int CurrentTicketIndex { get; private set; }

        public Frame SwipeZone { get; private set; } = new Frame()
        {
            BackgroundColor = Colors.Transparent,
            BorderColor = Colors.Transparent,
            VerticalOptions = LayoutOptions.FillAndExpand,
            HorizontalOptions = LayoutOptions.FillAndExpand,
        };

        public SwipingElement()
        {
            for (int i = 0; i < directions.Length; i++)
            {
                var currentDirection = directions[i];
                currentDirection.Command = new Command(async () => 
                {
                    CurrentSwipeDirection = currentDirection.Direction;
                    await UpdateAsync(); 
                });
                SwipeZone.GestureRecognizers.Add(currentDirection);
            }
        }

        public override Grid LayoutHandler { get; set; } = new Grid()
        {
            VerticalOptions = LayoutOptions.Start,
            WidthRequest = 1200,
            HeightRequest = 500,
        };

        public override void Inicialize(TicketHolder[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                var currentElement = new TicketElement();
                currentElement.Inicialize(data[i]);

                var baseFrame = new Frame()
                {
                    BackgroundColor = baseColor,
                    CornerRadius = 35,
                    HeightRequest = 300,
                    WidthRequest = 300,
                    ZIndex = i,
                    HasShadow = true,
                    Content = currentElement.LayoutHandler
                };

                LayoutHandler.Children.Add(baseFrame);
            }
            CurrentTicketIndex = (LayoutHandler.Children.Count - 1);

            SwipeZone.ZIndex = data.Length;
            LayoutHandler.Children.Add(SwipeZone);

            Task.Run(() => UpdateFrames());
        }

        protected override async Task UpdateAsync()
        {
            if (CurrentTicketIndex == (LayoutHandler.Children.Count - 2)) 
            {
                defaultSwipeDirection = CurrentSwipeDirection;
                int destinationX = (((int)CurrentSwipeDirection & 1) * (int)ticketDestinationPosition.X) - ((int)ticketDestinationPosition.X / 2);
                ticketPosition = new(destinationX, ticketDestinationPosition.Y);
            }
            bool defaultDirection = (int)defaultSwipeDirection != (int)CurrentSwipeDirection;
            CurrentTicketIndex += defaultDirection ? 1 : -1;

            if (!(defaultDirection) && CurrentTicketIndex <= -1)
                CurrentTicketIndex = 0;
            else 
            {
                await ExecuteSwipeAnimation(LayoutHandler.Children[CurrentTicketIndex] as Frame, defaultDirection);
            }
            await UpdateFrames();
        }

        private async Task UpdateFrames() 
        {
            for (int i = 0; i < CurrentTicketIndex; i++)
            {
                int currentIndex = (CurrentTicketIndex - i) - 1;
                var currentFrame = (LayoutHandler.Children[i] as Frame);
                double scaleX = 1 - (((double)currentIndex) / 10);
                _ = currentFrame.ScaleXTo(scaleX);

                int frameYPosition = ((int)(ticketDefaultPosition.Y)) - (currentIndex * 20);
                _ = currentFrame.TranslateTo(ticketDefaultPosition.X, frameYPosition);
            }
        }

        private async Task ExecuteSwipeAnimation(Frame currentFrame, bool isSwiped = false)
        {
            int index = isSwiped ? -1 : 1;
            int lastIndex = CurrentTicketIndex + index;
            var lastFrame = LayoutHandler[lastIndex] as Frame;

            int currentFrameZIndex = currentFrame.ZIndex;
            int lastFrameZIndex = lastFrame.ZIndex;

            currentFrame.ZIndex = currentFrameZIndex;
            lastFrame.ZIndex = lastFrameZIndex;

            if (isSwiped)
            {
                TrySwitchValue(ref currentFrameZIndex, ref lastFrameZIndex);
                await currentFrame.TranslateTo(ticketDefaultPosition.X, ticketDefaultPosition.Y);
            }
            else 
            {
                TrySwitchValue(ref lastFrameZIndex, ref currentFrameZIndex);
                await lastFrame.TranslateTo(ticketPosition.X, ticketPosition.Y);
            }

        }

        private bool TrySwitchValue(ref int valueOne, ref int valueTwo) 
        {
            if (valueOne == valueTwo)
                return false;

            int oldValueOne = valueOne;
            valueOne = valueTwo;
            valueTwo = oldValueOne;

            return true;
        }
    }
}
