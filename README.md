# [PerformanceTweaker](https://github.com/Fankserver/torchapi-performance-tweaker)

## MyLargeTurretBase

This will throttle the update rate for turrets to only update on each 100 ticks/ms instead of each tick/second.  
When a target is found, the turret will switch to a "hot" mode and the throttle is ignored.  
Until the target is lost for >= 300 ticks/ms, then it switches back to default "cold" mode.  

Throttle options:  
**Sim Speed**:  
When `ServerSimulationRatio` < `Threshold`, then throttle the update rate by `ServerSimulationRatio / Threshold`, instead of `1`.  
Examples:  
- `ServerSimulationRatio = 1` and `Threshold = 0.8`, the default throttle of 100 ticks/ms is used.  
- `ServerSimulationRatio = 0.7` and `Threshold = 0.8`, it will only update on each 114 ticks/ms.  
- `ServerSimulationRatio = 0.5` and `Threshold = 0.8`, it will only update on each 160 ticks/ms.  

**Sim CPU Load**:  
When `ServerCPULoad` > `Threshold`, then throttle the update rate by `Threshold / ServerCPULoad`, instead of `1`.
Examples:  
- `ServerCPULoad = 10` and `Threshold = 50`, the default throttle of 100 ticks/ms is used.  
- `ServerCPULoad = 70` and `Threshold = 50`, it will only update on each 140 ticks/ms.  
- `ServerCPULoad = 100` and `Threshold = 50`, it will only update on each 200 ticks/ms.  

**Static**:  
`10` means default update rate, any higher rate will cause a lower update rate.  
Examples:  
- `Threshold = 10`, the default throttle of 100 ticks/ms is used.  
- `Threshold = 20`, it will only update on each 200 ticks/ms.  
- `Threshold = 50`, it will only update on each 500 ticks/ms.  

**Disabled**:  
Disable turret updates, turrets will not work anymore!!!
