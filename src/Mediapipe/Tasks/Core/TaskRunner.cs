using Google.Protobuf;
using Mediapipe.Core;
using Mediapipe.Framework.Packet;
using Mediapipe.Gpu;
using Mediapipe.PInvoke;

namespace Mediapipe.Tasks.Core;

public class TaskRunner : MpResourceHandle
{
    public delegate void NativePacketsCallback(int name, IntPtr status, IntPtr packetMap);
    public delegate void PacketsCallback(PacketMap packetMap);

    public static TaskRunner Create(CalculatorGraphConfig config, GpuResources gpuResources, int callbackId = -1, NativePacketsCallback? packetsCallback = null)
    {
        var bytes = config.ToByteArray();
        var gpuResourcesPtr = gpuResources == null ? IntPtr.Zero : gpuResources.SharedPtr;
        UnsafeNativeMethods.mp_tasks_core_TaskRunner_Create__PKc_i_PF_Pgr(bytes, bytes.Length, callbackId, packetsCallback!, gpuResourcesPtr, out var statusPtr, out var taskRunnerPtr).Assert();

        AssertStatusOk(statusPtr);
        return new TaskRunner(taskRunnerPtr);
    }

    public static TaskRunner Create(CalculatorGraphConfig config, int callbackId = -1, NativePacketsCallback? packetsCallback = null)
    {
        var bytes = config.ToByteArray();
        UnsafeNativeMethods.mp_tasks_core_TaskRunner_Create__PKc_i_PF(bytes, bytes.Length, callbackId, packetsCallback!, out var statusPtr, out var taskRunnerPtr).Assert();

        AssertStatusOk(statusPtr);
        return new TaskRunner(taskRunnerPtr);
    }

    private TaskRunner(IntPtr ptr) : base(ptr) { }

    protected override void DeleteMpPtr()
    {
        UnsafeNativeMethods.mp_tasks_core_TaskRunner__delete(Ptr);
    }

    public PacketMap Process(PacketMap inputs)
    {
        UnsafeNativeMethods.mp_tasks_core_TaskRunner__Process__Ppm(MpPtr, inputs.MpPtr, out var statusPtr, out var packetMapPtr).Assert();
        inputs.Dispose(); // respect move semantics

        AssertStatusOk(statusPtr);
        return new PacketMap(packetMapPtr, true);
    }

    public void Send(PacketMap inputs)
    {
        UnsafeNativeMethods.mp_tasks_core_TaskRunner__Send__Ppm(MpPtr, inputs.MpPtr, out var statusPtr).Assert();
        inputs.Dispose(); // respect move semantics

        AssertStatusOk(statusPtr);
    }

    public void Close()
    {
        UnsafeNativeMethods.mp_tasks_core_TaskRunner__Close(MpPtr, out var statusPtr).Assert();

        AssertStatusOk(statusPtr);
    }

    public void Restart()
    {
        UnsafeNativeMethods.mp_tasks_core_TaskRunner__Restart(MpPtr, out var statusPtr).Assert();

        AssertStatusOk(statusPtr);
    }

    public CalculatorGraphConfig GetGraphConfig(ExtensionRegistry? extensionRegistry = null)
    {
        UnsafeNativeMethods.mp_tasks_core_TaskRunner__GetGraphConfig(MpPtr, out var serializedProto).Assert();

        var parser = extensionRegistry == null ? CalculatorGraphConfig.Parser : CalculatorGraphConfig.Parser.WithExtensionRegistry(extensionRegistry);
        var config = serializedProto.Deserialize(parser);
        serializedProto.Dispose();

        return config;
    }
}
