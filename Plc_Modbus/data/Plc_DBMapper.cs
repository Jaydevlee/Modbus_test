using Plc_Modbus.Config;
using Plc_Modbus.Model;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Plc_Modbus.data
{
    public class Plc_DBMapper
    {
        private AppConfig _appConfig = new AppConfig();
        private List<TagSetting> coilTags;
        private List<TagSetting> holdingTags;

        public Plc_DBMapper()
        {
            _appConfig = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText("config.json"));
            coilTags = _appConfig.TagSettings.CoilTag ?? new List<TagSetting>();
            holdingTags = _appConfig.TagSettings.HoldingTag ?? new List<TagSetting>();
        }

        public List<PlcDBDto> CoilMapping(bool[] readCoil)
        {
            List<PlcDBDto> _dbDto = new();
            foreach (var config in coilTags)
            {
                 _dbDto.Add(new PlcDBDto
                {
                    Equip_id = config.Equip_Id,
                    Address = config.Address,
                    Metric_name = config.Metric_Name,
                    Metric_value = readCoil[config.ArrayIndex] ? 1.0 : 0.0
                });
            }
            return _dbDto;
        }

        public List<PlcDBDto> HoldingMapping(ushort[] readHolding)
        {
            List<PlcDBDto> _dbDto = new();
            foreach (var config in holdingTags)
            {
                _dbDto.Add(new PlcDBDto
                {
                    Equip_id = config.Equip_Id,
                    Address = config.Address,
                    Metric_name = config.Metric_Name,
                    Metric_value = readHolding[config.ArrayIndex]
                });
            }
            return _dbDto;
        }
    }
}
