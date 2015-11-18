using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SWIG.BWAPI;
using System.Threading.Tasks;

namespace StarcraftBotLib
{
    public class BuildingQueueItem
    {
        public BuildingQueueItem(string type, TilePosition tile_position)
        {
            Type = type;
            TilePosition = tile_position;
            ActualPosition = tile_position;
            buildingTask = new TaskCompletionSource<BW.Unit>();
        }
        public string Type { get; set; }
        public TilePosition TilePosition { get; set; }
        // a field for "can't build here, i'll build here instead" types of tasks
        public TilePosition ActualPosition { get; set; }
        public TaskCompletionSource<BW.Unit> buildingTask;

        public async Task<BW.Unit> Build()
        {
            return await buildingTask.Task;
        }

        // Unnecessary helper
        public static TilePosition buildingPositionRelativeTo(string type, BW.Unit building, CustomTypes.Direction direction)
        {            
            TilePosition building_position = building.theUnit.getTilePosition();
            int x_offset = 0, y_offset = 0;
            switch (direction)
            {
                case CustomTypes.Direction.Left:
                    {
                        x_offset = -bwapi.getUnitType(type).tileWidth();
                        y_offset = 0;
                        break;
                    }
                case CustomTypes.Direction.Right:
                    {
                        x_offset = bwapi.getUnitType(building.Type).tileWidth();
                        y_offset = 0;
                        break;
                    }
                case CustomTypes.Direction.Down:
                    {
                        x_offset = 0;
                        y_offset = bwapi.getUnitType(building.Type).tileHeight();
                        break;
                    }
                case CustomTypes.Direction.Up:
                    {
                        x_offset = 0;
                        y_offset = -bwapi.getUnitType(type).tileHeight();
                        break;
                    }
            }
            return new TilePosition(building_position.xConst() + x_offset, building_position.yConst() + y_offset);
        }
    }
}
