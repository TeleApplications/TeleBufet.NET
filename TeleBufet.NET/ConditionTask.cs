
namespace TeleBufet.NET
{
    public static class ConditionTask
    {
        private const int OneSecond = 1000;

        public static async Task<bool> WaitUntil(Func<bool> condition, int waitTime) 
        {
            int currentTime = 0;
            var whileTask = Task<bool>.Run(async () =>
            {
                bool currentCondition = condition.Invoke();
                while (currentCondition) 
                {
                    await Task.Delay(OneSecond);
                    Interlocked.Increment(ref currentTime);

                    if (currentTime == waitTime)
                        return false;
                    currentCondition = condition.Invoke();
                }
                return true;
            });
            return await whileTask;
        }
    }
}
