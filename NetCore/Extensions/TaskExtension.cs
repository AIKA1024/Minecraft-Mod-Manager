namespace NetCore.Extensions;

public static class TaskExtension
{
  public static async void SafeFireAndForget(this Task task, Action? onComplete = null, Action<Exception>? onError = null)
  {
    try
    {
      await task;
      onComplete?.Invoke();
    }
    catch (Exception e)
    {
      onError?.Invoke(e);
    }
  }
}