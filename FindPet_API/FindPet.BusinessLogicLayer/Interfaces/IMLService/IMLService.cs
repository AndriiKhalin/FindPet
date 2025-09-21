namespace FindPet.BusinessLogicLayer.Interfaces.IMLService;

public interface IMLService
{
    Task<string> PredictAsync(string filePath);
}