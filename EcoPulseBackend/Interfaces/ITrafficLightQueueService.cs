using EcoPulseBackend.Models.Result;
using EcoPulseBackend.Models.TrafficLightQueue;

namespace EcoPulseBackend.Interfaces;

/// <summary>
/// Сервис стоящего транспорта на регулируемом перекрестке
/// </summary>
public interface ITrafficLightQueueService
{
    /// <summary>
    /// Расчет выбросов по группе загрязнителей
    /// </summary>
    /// <param name="model">Модель для расчета выбросов стоящего транспорта на регулируемом перекрестке</param>
    /// <returns></returns>
    public List<EmissionsResult> CalculateEmissionsBatch(TrafficLightQueueEmissionsCalculateModel model);
}