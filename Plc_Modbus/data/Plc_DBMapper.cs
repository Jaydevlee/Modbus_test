using Plc_Modbus.Config;
using Plc_Modbus.Model;

namespace Plc_Modbus.data
{
    public class Plc_DBMapper
    {
        private readonly IReadOnlyList<TagSetting> _holdingTags;

        public Plc_DBMapper()
        {
            AppConfig config = AppConfigLoader.Load();
            _holdingTags = config.TagSettings?.HoldingTag ?? [];
        }

        public List<PlcDBDto> HoldingMapping(ushort[] registers)
        {
            List<PlcDBDto> result = [];
            foreach (TagSetting tag in _holdingTags.Where(tag => tag.Enabled))
            {
                double rawValue = ReadRawValue(registers, tag);
                result.Add(new PlcDBDto
                {
                    Equip_id = tag.Equip_Id,
                    Address = tag.Address,
                    Metric_name = tag.Metric_Name,
                    Metric_value = rawValue * tag.Scale,
                    Unit = tag.Unit,
                    Quality = 192 // 품질 값은 필요에 따라 설정
                });
            }

            return result;
        }

        private static double ReadRawValue(ushort[] registers, TagSetting tag)
        {
            if (tag.ArrayIndex < 0 || tag.ArrayIndex >= registers.Length)
            {
                throw new InvalidOperationException(
                    $"태그 {tag.Metric_Name}의 ArrayIndex가 읽기 범위를 벗어났습니다.");
            }

            if (tag.DataType.Equals("UInt32", StringComparison.OrdinalIgnoreCase))
            {
                if (tag.ArrayIndex + 1 >= registers.Length)
                {
                    throw new InvalidOperationException(
                        $"UInt32 태그 {tag.Metric_Name}에는 레지스터 2개가 필요합니다.");
                }

                return ((uint)registers[tag.ArrayIndex] << 16)
                       | registers[tag.ArrayIndex + 1];
            }

            return registers[tag.ArrayIndex];
        }
    }
}
