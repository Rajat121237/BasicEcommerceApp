namespace eCommerce.API.Simulation;

public static class ErrorSimulator
{
    public static async Task GetError()
    {
        await Task.Delay(1000); 
        throw new Exception("An error occurred while processing your request.");
    }

    public static async Task AddDelayInMilliseconds(int delayInMilliseconds)
    {
        await Task.Delay(delayInMilliseconds);
    }
}
