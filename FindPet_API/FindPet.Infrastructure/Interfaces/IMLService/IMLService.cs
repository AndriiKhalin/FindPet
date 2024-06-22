namespace FindPet.Infrastructure.Interfaces.IMLService;

public interface IMLService
{
    Task<string> PredictAsync(string filePath);
}