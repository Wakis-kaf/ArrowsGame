namespace Framework.Runtime.ExceptionSystem
{
    /*
     错误码定义规则：
     环境（底层环境错误、引擎错误、插件错误、框架错误、逻辑错误、未知错误）
     错误原因
     优先级（Ignore、Low、Medium、High、Fatal）
     */

    public enum EnvironmentErrorCode
    {
        Environment = 1,
        Engine = 2,
        Plugin = 3,
        Framework = 4,
        Logic = 4,
        UnKnow = 4,
    }

    public enum ErrorLevel
    {
        Ignore,
        Low,
        Medium,
        High,
        Fatal,
        Emergency // 紧急，需要立刻解决
    }

    public enum ErrorRes
    {
        ILLEGAL_INPUT_VALUE,// 非法输入值,
        ERROR_INPUT_TYPE, // 输入值错误
        NET_CONNECT_TIME_OUT, // 网络超时
        NET_CONNECT_REPEAT, // 网络重复连接
        NET_CONNECT_DISCONNECT, // 网络断开
        NET_CONNECT_ERROR, // 网络连接错误
        NET_DISCONNECT_ERROR, // 网络断开连接错误
        FILE_OPEN_ERROR,
        FILE_WRITE_ERROR,
        FILE_CREATE_ERROR,
        FILE_OPERATE_ERROR,
        RESOURCES_UPDATE_ERROR,
        RESOURCES_LOAD_ERROR,
        RESOURCES_NOT_EXIST_ERROR,
        RESOURCES_TYPE_ERROR,
        PROGRAM_LOGIC_ERROR,
        PROGRAM_VALUE_TYPE_ERROR,
        PROGRAM_NULL_REFERENCE_ERROR,
        SYSTEM_OS_ERROR,
        SYSTEM_HARDWARE_ERROR,
        MEMORY_OUTOF_ERROR, // 内存不足
        MEMORY_LEAKS_ERROR, // 内存泄露
        CUSTOM,
    }

    public class ExceptionCode
    {
    }
}