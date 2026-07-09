using System;
using GridsSpaceEditor.Data.Enums;

namespace GridsSpaceEditor.Data.Models
{
    [Serializable]
    public class PortInstance
    {
        public string PortID = "201";
        public EdgeSide Side = EdgeSide.顶部;
        public PortIOType IOType = PortIOType.输入;
        public string PresetName = "手动修改";
        public string InputFilter = "*";
        public string InputDescription = "";
        public string OutputType = "Trigger";
        public string OutputDescription = "";
        public string PortDescription = "";

        public static PortInstance Clone(PortInstance source, string presetName)
        {
            return new PortInstance
            {
                PortID = source.PortID,
                Side = source.Side,
                IOType = source.IOType,
                PresetName = presetName,
                InputFilter = source.InputFilter,
                InputDescription = source.InputDescription,
                OutputType = source.OutputType,
                OutputDescription = source.OutputDescription,
                PortDescription = source.PortDescription
            };
        }

        public void SyncFrom(PortInstance other, string presetName)
        {
            PortID = other.PortID;
            IOType = other.IOType;
            PresetName = presetName;
            InputFilter = other.InputFilter;
            InputDescription = other.InputDescription;
            OutputType = other.OutputType;
            OutputDescription = other.OutputDescription;
            PortDescription = other.PortDescription;
        }
    }
}
