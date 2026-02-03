using EcoPulseBackend.Interfaces;

namespace EcoPulseBackend.Services;

public class EmissionService : IEmissionService
{
    public IGasolineGeneratorService GasolineGeneratorService { get; }
    public IReservoirsService ReservoirsService { get; }
    public IDuringMetalMachiningService DuringMetalMachiningService { get; }
    public IDuringWeldingOperationsService DuringWeldingOperationsService { get; }
    public IMaximumSingleService MaximumSingleService { get; }
    public IVehicleFlowService VehicleFlowService { get; }
    public ITrafficLightQueueService TrafficLightQueueService { get; }
    public IOpenCoalWarehouseService OpenCoalWarehouseService { get; }

    public EmissionService(IMaximumSingleService maximumSingleService, 
        IGasolineGeneratorService gasolineGeneratorService, 
        IReservoirsService reservoirsService, 
        IDuringMetalMachiningService duringMetalMachiningService, 
        IDuringWeldingOperationsService duringWeldingOperationsService, 
        IVehicleFlowService vehicleFlowService, 
        ITrafficLightQueueService trafficLightQueueService, 
        IOpenCoalWarehouseService openCoalWarehouseService)
    {
        MaximumSingleService = maximumSingleService;
        GasolineGeneratorService = gasolineGeneratorService;
        ReservoirsService = reservoirsService;
        DuringMetalMachiningService = duringMetalMachiningService;
        DuringWeldingOperationsService = duringWeldingOperationsService;
        VehicleFlowService = vehicleFlowService;
        TrafficLightQueueService = trafficLightQueueService;
        OpenCoalWarehouseService = openCoalWarehouseService;
    }
}