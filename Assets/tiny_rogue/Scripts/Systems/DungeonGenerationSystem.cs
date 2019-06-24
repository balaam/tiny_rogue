using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace game
{
    [AlwaysUpdateSystem]
    public class DungeonGenerationSystem : ComponentSystem
    {
        private enum HallDirection
        {
            Horizontal,
            Vertical
        }

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
        }

        List<Room> _rooms = new List<Room>();
        Entity _dungeonViewEntity;
        View _view;

        bool updateTileComponents = false;

        protected override void OnUpdate()
        {
            if (updateTileComponents)
            {
                Entities.WithAll<Tile>().ForEach((Entity e, ref Sprite2DRenderer renderer) =>
                    {
                        if (renderer.sprite == SpriteSystem.AsciiToSprite['D'])
                            PostUpdateCommands.AddComponent<Door>(e, new Door());
                        if (renderer.sprite == SpriteSystem.AsciiToSprite['#'])
                            PostUpdateCommands.AddComponent<BlockMovement>(e, new BlockMovement());
                    });

                updateTileComponents = false;
            }
        }

        public void GenerateDungeon(View view)
        {
            if (!SpriteSystem.Loaded)
                return;

            updateTileComponents = false;
            _view = view;

            int maxRoom = 0;
            int maxRoomSize = 0;
            int minRoomSize = 0;

            Entities.WithAll<DungeonGenerator>().ForEach((Entity e, ref DungeonGenerator gen) =>
            {
                maxRoom = gen.MaxNumberOfRooms;
                maxRoomSize = gen.MaxRoomSize;
                minRoomSize = gen.MinRoomSize;

            });

            for (int i = 0; i < maxRoom; i++)
            {
                int newWidth = RandomRogue.Next(minRoomSize, maxRoomSize);
                int newHeight = RandomRogue.Next(minRoomSize, maxRoomSize);

                int newX = RandomRogue.Next(1, _view.Width - newWidth);
                int newY = RandomRogue.Next(1, _view.Height - newHeight);


                Room newRoom = new Room(newX, newY, newWidth, newHeight);
                if (!RoomIntersectExisitngRooms(newRoom))
                    _rooms.Add(newRoom);
            }

            ClearCurrentLevel();
            CreateRooms();
            CreateHallways();

            updateTileComponents = true;
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
                        Sprite2DRenderer renderer = EntityManager.GetComponentData<Sprite2DRenderer>(_view.ViewTiles[tileIndex]);

                        bool isWall = (i == room.startX
                            || i == room.startX + room.width - 1
                            || j == room.startY
                            || j == room.startY + room.height - 1);

                        if (isWall)
                        {
                            renderer.sprite = SpriteSystem.AsciiToSprite['#'];
                        }
                        else
                        {
                            renderer.sprite = SpriteSystem.AsciiToSprite['.'];
                        }

                        EntityManager.SetComponentData<Sprite2DRenderer>(_view.ViewTiles[tileIndex], renderer);
                    }
                }
            }
        }

        public int2 GetPlayerStartPosition()
        {
            Room startRoom = _rooms[RandomRogue.Next(0, _rooms.Count)];
            return startRoom.GetCenterTile();
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
                    CreateHorizontalHall(room1Pos.y, room1Pos.x, room2Pos.x);
                    CreateVerticalHallway(room2Pos.x, room1Pos.y, room2Pos.y);
                }
                //start vertical
                else
                {
                    CreateVerticalHallway(room1Pos.x, room1Pos.y, room2Pos.y);
                    CreateHorizontalHall(room2Pos.y, room1Pos.x, room2Pos.x);
                }
            }
        }

        private void CreateHorizontalHall(int y, int from, int to)
        {
            int currentX = from;
            if (currentX < to)
            {
                while (currentX < to)
                {
                    int2 xy = new int2(currentX, y);
                    CreateHallwayTile(xy, HallDirection.Horizontal);
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
            else
            {
                while (currentX > to)
                {
                    int2 xy = new int2(currentX, y);
                    CreateHallwayTile(xy, HallDirection.Horizontal);
                    CreateWallsIfEmpty(
                        new int2(currentX, y + 1),
                        new int2(currentX, y - 1),
                        new int2(currentX + 1, y + 1),
                        new int2(currentX - 1, y - 1),
                        new int2(currentX + 1, y - 1),
                        new int2(currentX - 1, y + 1));
                    currentX--;
                }
            }
        }

        private void CreateVerticalHallway(int x, int from, int to)
        {
            int currentY = from;
            if (currentY < to)
            {
                while (currentY < to)
                {
                    int2 xy = new int2(x, currentY);
                    CreateHallwayTile(xy, HallDirection.Vertical);
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
            else
            {
                while (currentY > to)
                {
                    int2 xy = new int2(x, currentY);
                    CreateHallwayTile(xy, HallDirection.Vertical);
                    CreateWallsIfEmpty(
                       new int2(x + 1, currentY),
                       new int2(x - 1, currentY),
                       new int2(x + 1, currentY + 1),
                       new int2(x - 1, currentY - 1),
                       new int2(x + 1, currentY - 1),
                       new int2(x - 1, currentY + 1));
                    currentY--;
                }
            }
        }

        private void CreateWallsIfEmpty(params int2[] positions)
        {
            foreach(int2 pos in positions)
            {
                int tileIndex = View.XYToIndex(pos, _view.Width);
                Sprite2DRenderer renderer = EntityManager.GetComponentData<Sprite2DRenderer>(_view.ViewTiles[tileIndex]);
                
                //only add if the space is blank
                if (renderer.sprite == SpriteSystem.AsciiToSprite[' '])
                {
                    renderer.sprite = SpriteSystem.AsciiToSprite['#'];
                    EntityManager.SetComponentData<Sprite2DRenderer>(_view.ViewTiles[tileIndex], renderer);
                }
            }
        }

        private bool RoomIntersectExisitngRooms(Room roomToAdd)
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
            //Clear the map
            Entities.WithAll<Tile>().ForEach((Entity entity, ref Sprite2DRenderer renderer) =>
            {
                renderer.sprite = SpriteSystem.AsciiToSprite[' '];
            });

            //Clear all walls
            Entities.WithAll<BlockMovement, Tile>().ForEach((Entity entity) =>
            {
                EntityManager.RemoveComponent(entity, typeof(BlockMovement));
            });
        }

        private void SetTileToChar(int2 pos, char c)
        {
            int tileIndex = View.XYToIndex(pos, _view.Width);
            Sprite2DRenderer renderer = EntityManager.GetComponentData<Sprite2DRenderer>(_view.ViewTiles[tileIndex]);
            renderer.sprite = SpriteSystem.AsciiToSprite[c];
            EntityManager.SetComponentData<Sprite2DRenderer>(_view.ViewTiles[tileIndex], renderer);
        }

        private void CreateHallwayTile(int2 xy, HallDirection direction)
        {
            var entity = _view.ViewTiles[View.XYToIndex(xy, _view.Width)];

            int2 neighbor1 = xy;
            int2 neighbor2 = xy;

            if(direction == HallDirection.Horizontal)
            {
                neighbor1.y += 1;
                neighbor2.y -= 1;
            }
            else
            {
                neighbor1.x += 1;
                neighbor2.x -= 1;
            }

            var neighborEntityOne = _view.ViewTiles[View.XYToIndex(neighbor1, _view.Width)];
            var neighborEntityTwo = _view.ViewTiles[View.XYToIndex(neighbor2, _view.Width)];

            bool tileIsWall = EntityManager.GetComponentData<Sprite2DRenderer>(entity).sprite == SpriteSystem.AsciiToSprite['#'];
            bool neighbor1IsWall = EntityManager.GetComponentData<Sprite2DRenderer>(neighborEntityOne).sprite == SpriteSystem.AsciiToSprite['#'];
            bool neighbor2IsWall = EntityManager.GetComponentData<Sprite2DRenderer>(neighborEntityTwo).sprite == SpriteSystem.AsciiToSprite['#'];

            if(tileIsWall && neighbor1IsWall && neighbor2IsWall)
                SetTileToChar(xy, 'D');
            else
                SetTileToChar(xy, '.');
        }
    }
}
