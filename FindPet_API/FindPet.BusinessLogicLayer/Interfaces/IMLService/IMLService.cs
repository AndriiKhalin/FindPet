namespace FindPet.BusinessLogicLayer.Interfaces.IMLService;

public interface IMLService
{
    Task<string> PredictAsync(string filePath);

    Task<string> PredictAsync(Stream imageStream);
}