using Config;
using Config.Data;
using Controllers;
using StarPlatinum;

namespace Game
{
    public interface IGameRuntimeData
    {
        ControllerManager ControllerManager { get; }
        ConfigProvider ConfigProvider { get; }
        SoundService SoundService { get; }
        ConfigDataProvider ConfigDataProvider { get; }
    }
}