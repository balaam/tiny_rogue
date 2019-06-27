using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    [AlwaysUpdateSystem]
    public class DungeonSystem : ComponentSystem
    {
        private struct Room
        {
            public int startX;
            public int startY;
            public int width;
            public int height;

            public Room(int x, int y, int w, int h)
            {
                startX = x;
                startY = y;
                width = w;
                height = h;
            }

            public int2 GetCenterTile()
            {
                int2 pos = new int2((startX + width / 2), (startY + height / 2));
                return pos;
            }

            public int2 GetRandomTile()
            {
                int2 pos = new int2(RandomRogue.Next(startX + 1, startX + width - 1), RandomRogue.Next(startY + 1, startY + height - 1));
                return pos;
            }
        }

        enum Type
        {
            eEmpty,
            eWall,
            eFloor,
            eHallway,
            eDoor
        }

        // Storage for created rooms
        private Type[] _cells = new Type[0];
        private List<Room> _rooms = new List<Room>();
        private List<int2> _verticalDoors = new List<int2>();
        private List<int2> _horizontalDoors = new List<int2>();
        
        private List<int2> _collectiblesCoords = new List<int2>();

        private Entity _dungeonViewEntity;
        private View _view;

        private EntityCommandBuffer _ecb;
        private CreatureLibrary _creatureLibrary;
        ArchetypeLibrary _archetypeLibrary;

        int numberOfCollectibles = 0;
        public int NumberOfCollectibles => numberOfCollectibles;

        protected override void OnUpdate() {}

        public void ClearDungeon(EntityCommandBuffer cb, View view)
        {
            _ecb = cb;
            _view = view;

            _rooms.Clear();
            _verticalDoors.Clear();
            _horizontalDoors.Clear();
            for (var i = 0; i < _cells.Length; i++)
                _cells[i] = Type.eEmpty;

            ClearCurrentLevel();
        }

        public void GenerateDungeon(EntityCommandBuffer cb, View view, CreatureLibrary cl, ArchetypeLibrary al, int level, bool lastLevel)
        {
            _ecb = cb;
            _view = view;
            _creatureLibrary = cl;
            _archetypeLibrary = al;

            ClearDungeon(cb, view);
            
            _cells = new Type[_view.ViewTiles.Length];

            int maxRoom = 0;
            int maxRoomSize = 0;
            int minRoomSize = 0;

            Entities.WithAll<DungeonGenerator>().ForEach((Entity e, ref DungeonGenerator gen) =>
            {
                maxRoom = gen.MaxNumberOfRooms;
                maxRoomSize = gen.MaxRoomSize;
                minRoomSize = gen.MinRoomSize;
                numberOfCollectibles = gen.NumberOfCollectibles;
            });

            for (int i = 0; i < maxRoom; i++)
            {
                int newWidth = RandomRogue.Next(minRoomSize, maxRoomSize);
                int newHeight = RandomRogue.Next(minRoomSize, maxRoomSize);

                int newX = RandomRogue.Next(1, _view.Width - newWidth);
                int newY = RandomRogue.Next(1, _view.Height - newHeight);


                Room newRoom = new Room(newX, newY, newWidth, newHeight);
                if (!RoomIntersectExistingRooms(newRoom))
                    _rooms.Add(newRoom);
            }

            // Create the rooms, and then the hallways
            CreateRooms();
            CreateHallways();
            CreateDoors();
            CreateTraps();
            CreateGold();
            CreateCollectibles();
            CreateHealingItems();

            // TODO: Add loot - this is added in GenerateCollectible in GameSystem
            PlaceCreatures();

            PlaceDungeon();
            PlacePlayer(level == 1);
            PlaceExit(lastLevel);
        }

        private void PlaceExit(bool lastLevel)
        {
            if (lastLevel)
            {
                var crownCoord = GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateCrown(_ecb, crownCoord, _view.ViewCoordToWorldPos(crownCoord));
            }
            else
            {
                var stairwayCoord = GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateStairway(_ecb, stairwayCoord,
                    _view.ViewCoordToWorldPos(stairwayCoord));
            }
        }

        private void PlaceCreatures()
        {
            int minCreatureCount = 4;
            int maxCreatureCount = 12;
            
            int creatureCount = RandomRogue.Next(minCreatureCount, maxCreatureCount);
            int creatureTypeCount = (int)ECreatureId.SpawnableCount;
            for (int i = 0; i < creatureCount; i++)
            {
                int creatureId = RandomRogue.Next(0, creatureTypeCount);
                var worldCoord = GetRandomPositionInRandomRoom();
                var viewCoord = _view.ViewCoordToWorldPos(worldCoord);
                Entity cr = _creatureLibrary.SpawnCreature(_ecb, (ECreatureId)creatureId);

                _ecb.SetComponent(cr, new WorldCoord {x = worldCoord.x, y = worldCoord.y});
                _ecb.SetComponent(cr, new Translation {Value = viewCoord});
                _ecb.SetComponent(cr, new PatrollingState {destination = GetRandomPositionInRandomRoom()});
            }
        }

        private void PlacePlayer(bool reset)
        {
            // Place the player
            Entities.WithAll<PlayerInput>().ForEach((Entity player) =>
            {
                int2 randomStartPosition = GetPlayerStartPosition();
                WorldCoord worldCoord = new WorldCoord {x = randomStartPosition.x, y = randomStartPosition.y};
                Translation translation = new Translation {Value = _view.ViewCoordToWorldPos(randomStartPosition)};
                if (reset)
                {
                    _creatureLibrary.ResetPlayer(_ecb, player, worldCoord, translation);
                }
                else
                {
                    _ecb.SetComponent(player, worldCoord);
                    _ecb.SetComponent(player, translation);
                }
            });
        }

        private void PlaceDungeon()
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                switch (_cells[i])
                {
                    case Type.eWall:
                        _ecb.AddComponent(_view.ViewTiles[i], new Wall());
                        _ecb.AddComponent(_view.ViewTiles[i], new BlockMovement());
                        break;
                    case Type.eHallway:
                    case Type.eFloor:
                    case Type.eDoor:
                        _ecb.AddComponent(_view.ViewTiles[i], new Floor());
                        break;
                    case Type.eEmpty:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Placing unknown type");
                }
            }
        }
        
        void CreateHealingItems()
        {
            int healingItems = RandomRogue.Next(0, 5);
            for (int i = 0; i < healingItems; i++)
            {
                var healCoord = GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateHealingItem(_ecb, healCoord, _view.ViewCoordToWorldPos(healCoord),
                    RandomRogue.Next(-2, 6));
            }
        }
        
        void CreateCollectibles()
        { 
            for (int i = 0; i < NumberOfCollectibles; i++)
            {
                //TODO: figure out how it can know to avoid tiles that already have an entity
                var collectibleCoord = GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateCollectible(_ecb, collectibleCoord, _view.ViewCoordToWorldPos(collectibleCoord));
            }
        }

        private void CreateGold()
        {
            // Saving the num in a variable so it can be used for
            // the replay system, if need be
            int goldPiles = RandomRogue.Next(10);
            for (int i = 0; i < goldPiles; i++)
            {
                //TODO: figure out how it can know to avoid tiles that already have an entity
                var goldCoord = GetRandomPositionInRandomRoom();
                _archetypeLibrary.CreateGold(_ecb, goldCoord, _view.ViewCoordToWorldPos(goldCoord));
            }
        }

        private void CreateTraps()
        {
            
            // Hard code a couple of spear traps, so the player can die.
            var trap1Coord = GetRandomPositionInRandomRoom();
            var trap2Coord = GetRandomPositionInRandomRoom();
            _archetypeLibrary.CreateSpearTrap(_ecb, trap1Coord, _view.ViewCoordToWorldPos(trap1Coord));
            _archetypeLibrary.CreateSpearTrap(_ecb, trap2Coord, _view.ViewCoordToWorldPos(trap2Coord));
        }

        private void CreateRooms()
        {
            foreach (Room room in _rooms)
            {
                for (int i = room.startX; i < room.startX + room.width; i++)
                {
                    for (int j = room.startY; j < room.startY + room.height; j++)
                    {
                        int2 xy = new int2(i, j);
                        int tileIndex = View.XYToIndex(xy, _view.Width);

                        bool isWall = (i == room.startX
                            || i == room.startX + room.width - 1
                            || j == room.startY
                            || j == room.startY + room.height - 1);

                        if(isWall)
                            _cells[tileIndex] = Type.eWall;
                        else
                            _cells[tileIndex] = Type.eFloor;
                    }
                }
            }
        }

        public int2 GetPlayerStartPosition()
        {
            Room startRoom = _rooms[RandomRogue.Next(0, _rooms.Count)];
            return startRoom.GetRandomTile();
        }

        private void CreateHallways()
        {
            for(int i = 0; i < _rooms.Count - 1; i++)
            {
                int2 room1Pos = _rooms[i].GetCenterTile();
                int2 room2Pos = _rooms[i + 1].GetCenterTile();

                int initialHallwayDirection = RandomRogue.Next(0, 1);

                //start horizontal
                if (initialHallwayDirection == 0)
                {
                    CreateHorizontalHallway(room1Pos.y, room1Pos.x, room2Pos.x);
                    CreateVerticalHallway(room2Pos.x, room1Pos.y, room2Pos.y);
                }
                //start vertical
                else
                {
                    CreateVerticalHallway(room1Pos.x, room1Pos.y, room2Pos.y);
                    CreateHorizontalHallway(room2Pos.y, room1Pos.x, room2Pos.x);
                }
            }
        }

        private void CreateHorizontalHallway(int y, int _from, int _to)
        {
            var from = Math.Min(_from, _to);
            var to = Math.Max(_from, _to);
            int currentX = from;
            while (currentX <= to)
            {
                var xy = new int2(currentX, y);
                CreateHallwayTile(xy);
                CreateWallsIfEmpty(
                    new int2(currentX, y + 1),
                    new int2(currentX, y - 1),
                    new int2(currentX + 1, y + 1),
                    new int2(currentX - 1, y - 1),
                    new int2(currentX + 1, y - 1),
                    new int2(currentX - 1, y + 1));
                currentX++;
            }
        }

        private void CreateVerticalHallway(int x, int _from, int _to)
        {
            var from = Math.Min(_from, _to);
            var to = Math.Max(_from, _to);
            int currentY = from;
            while (currentY <= to)
            {
                var xy = new int2(x, currentY);
                CreateHallwayTile(xy);
                CreateWallsIfEmpty(
                    new int2(x + 1, currentY),
                    new int2(x - 1, currentY),
                    new int2(x + 1, currentY + 1),
                    new int2(x - 1, currentY - 1),
                    new int2(x + 1, currentY - 1),
                    new int2(x - 1, currentY + 1));
                currentY++;
            }
        }

        private void CreateWallsIfEmpty(params int2[] positions)
        {
            foreach (var pos in positions)
            {
                var tileIndex = View.XYToIndex(pos, _view.Width);
                if (_cells[tileIndex] == Type.eEmpty)
                    _cells[tileIndex] = Type.eWall;
            }
        }

        private bool RoomIntersectExistingRooms(Room roomToAdd)
        {
            foreach (Room createdRoom in _rooms)
            {
                Rect r1 = new Rect(roomToAdd.startX, roomToAdd.startY, roomToAdd.width, roomToAdd.height);
                Rect r2 = new Rect(createdRoom.startX, createdRoom.startY, createdRoom.width, createdRoom.height);

                if ((r1.x < r2.x + r2.width) &&
                    (r1.x + r1.width > r2.x) &&
                    (r1.y < r2.y + r2.height) &&
                    (r1.y + r1.height > r2.y))
                    return true;
            }
            return false;
        }

        private void ClearCurrentLevel()
        {
            // Clear each of our level tile tags
            Entities.WithAll<Tile,BlockMovement>().ForEach((Entity e) =>_ecb.RemoveComponent<BlockMovement>(e));
            Entities.WithAll<Tile,Wall>().ForEach((Entity e) =>_ecb.RemoveComponent<Wall>(e));
            Entities.WithAll<Tile,Floor>().ForEach((Entity e) =>_ecb.RemoveComponent<Floor>(e));
        }

        public int2 GetRandomPositionInRandomRoom()
        {
            Room room = _rooms[RandomRogue.Next(0, _rooms.Count)];
            return room.GetRandomTile();
        }

        public List<int2> GetHorizontalDoors()
        {
            return _horizontalDoors;
        }

        public List<int2> GetVerticalDoors()
        {
            return _verticalDoors;
        }

        private void CreateHallwayTile(int2 xy)
        {
            var current = _cells[View.XYToIndex(xy, _view.Width)];

            if (current != Type.eFloor)
            {
                _cells[View.XYToIndex(xy, _view.Width)] = Type.eHallway;
            }
        }

        void CreateDoors()
        {
            for (var i = 0; i < _cells.Length; i++)
            {
                var current = _cells[i];
                if (current != Type.eHallway) continue;
                
                var xy = View.IndexToXY(i, _view.Width);

                var neighbourUpXy = xy;
                neighbourUpXy.y--;
                var neighbourUp = _cells[View.XYToIndex(neighbourUpXy, _view.Width)];
                
                var neighbourDownXy = xy;
                neighbourDownXy.y++;
                var neighbourDown = _cells[View.XYToIndex(neighbourDownXy, _view.Width)];
                
                var neighbourRightXy = xy;
                neighbourRightXy.x++;
                var neighbourRight = _cells[View.XYToIndex(neighbourRightXy, _view.Width)];
                
                var neighbourLeftXy = xy;
                neighbourLeftXy.x--;
                var neighbourLeft = _cells[View.XYToIndex(neighbourLeftXy, _view.Width)];
                
                var horizontal = false;
                var vertical = false;
                
                if (neighbourLeft == Type.eHallway && 
                    neighbourRight == Type.eFloor &&
                    neighbourUp == Type.eWall &&
                    neighbourDown == Type.eWall)
                {
                    horizontal = true;
                }
                if (neighbourRight == Type.eHallway && 
                    neighbourLeft == Type.eFloor &&
                    neighbourUp == Type.eWall &&
                    neighbourDown == Type.eWall)
                {
                    horizontal = true;
                }
                if (neighbourUp == Type.eHallway && 
                    neighbourDown == Type.eFloor &&
                    neighbourRight == Type.eWall &&
                    neighbourLeft == Type.eWall)
                {
                    vertical = true;
                }
                if (neighbourDown == Type.eHallway && 
                    neighbourUp == Type.eFloor &&
                    neighbourRight == Type.eWall &&
                    neighbourLeft == Type.eWall)
                {
                    vertical = true;
                }

                if (horizontal || vertical)
                {
                    _cells[i] = Type.eDoor;
                }

                if (vertical)
                {
                    _verticalDoors.Add(xy);
                }

                if (horizontal)
                {
                    _verticalDoors.Add(xy);
                }
            }
            
            // Apply doors
            foreach (var doorCoord in _horizontalDoors)
            {
                if (RandomRogue.Next(TinyRogueConstants.DoorProbability) == 0)
                {
                    _archetypeLibrary.CreateDoorway(_ecb, doorCoord, _view.ViewCoordToWorldPos(doorCoord), true);
                }
            }
            foreach (var doorCoord in _verticalDoors)
            {
                if (RandomRogue.Next(TinyRogueConstants.DoorProbability) == 0)
                {
                    _archetypeLibrary.CreateDoorway(_ecb, doorCoord, _view.ViewCoordToWorldPos(doorCoord), false);
                }
            }
        }
    }
}
