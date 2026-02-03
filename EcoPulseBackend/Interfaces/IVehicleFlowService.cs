using EcoPulseBackend.Models.Result;
using EcoPulseBackend.Models.VehicleFlow;

namespace EcoPulseBackend.Interfaces;

/// <summary>
/// Сервис движущегося транспорта
/// </summary>
public interface IVehicleFlowService
{
    /// <summary>
    /// Расчет выбросов по группе загрязнителей
    /// </summary>
    /// <param name="model">Модель для расчета выбросов движущегося транспорта</param>
    /// <returns></returns>
    public List<EmissionsResult> CalculateEmissionsBatch(VehicleFlowEmissionsCalculateModel model);
}