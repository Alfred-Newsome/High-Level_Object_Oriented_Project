namespace High_level_Object_Oriented_Project
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: Quadtree <path to cmmd file>");
                return;
            }

            string filename = args[0];
            var processor = new CommandProcessor(filename);
        }
    }
    public class Rectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }

        public Rectangle(int x, int y, int length, int width)
        {
            X = x;
            Y = y;
            Length = length;
            Width = width;
        }

        public override string ToString()
        {
            return $"Rectangle at {X}, {Y}: {Length}x{Width}";
        }
    }
    public abstract class Node
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        public Node(int minX, int maxX, int minY, int maxY)
        {
            MinX = minX;
            MaxX = maxX;
            MinY = minY;
            MaxY = maxY;
        }

        public abstract void Dump(int level);
        public abstract bool Insert(Rectangle rect);
        public abstract bool Delete(int x, int y);
        public abstract Rectangle Find(int x, int y);
    }
    public class LeafNode : Node
    {
        private List<Rectangle> rectangles;

        public LeafNode(int minX, int maxX, int minY, int maxY) : base(minX, maxX, minY, maxY)
        {
            rectangles = new List<Rectangle>();
        }

        public override bool Insert(Rectangle rect)
        {
            if (rectangles.Count >= 5)
            {
                return false;
            }
            rectangles.Add(rect);
            return true;
        }

        public override bool Delete(int x, int y)
        {
            var rect = rectangles.FirstOrDefault(r => r.X == x && r.Y == y);
            if (rect == null)
            {
                return false;
            }
            rectangles.Remove(rect);
            return true;
        }

        public override Rectangle Find(int x, int y)
        {
            return rectangles.FirstOrDefault(r => r.X == x && r.Y == y);
        }

        public override void Dump(int level)
        {
            foreach (var rect in rectangles)
            {
                Console.WriteLine(new string('\t', level) + rect.ToString());
            }
        }
    }
    public class InternalNode : Node
    {
        public Node[] Children { get; set; }

        public InternalNode(int minX, int maxX, int minY, int maxY) : base(minX, maxX, minY, maxY)
        {
            Children = new Node[4];
        }

        public override bool Insert(Rectangle rect)
        {
            int midX = (MinX + MaxX) / 2;
            int midY = (MinY + MaxY) / 2;

            // Determine which quadrant the rectangle belongs to
            if (rect.X < midX && rect.Y >= midY) // top-left
            {
                if (Children[0] == null) Children[0] = new LeafNode(MinX, midX, midY, MaxY);
                return Children[0].Insert(rect);
            }
            if (rect.X >= midX && rect.Y >= midY) // top-right
            {
                if (Children[1] == null) Children[1] = new LeafNode(midX, MaxX, midY, MaxY);
                return Children[1].Insert(rect);
            }
            if (rect.X < midX && rect.Y < midY) // bottom-left
            {
                if (Children[2] == null) Children[2] = new LeafNode(MinX, midX, MinY, midY);
                return Children[2].Insert(rect);
            }
            if (rect.X >= midX && rect.Y < midY) // bottom-right
            {
                if (Children[3] == null) Children[3] = new LeafNode(midX, MaxX, MinY, midY);
                return Children[3].Insert(rect);
            }

            return false;
        }

        public override bool Delete(int x, int y)
        {
            foreach (var child in Children)
            {
                if (child != null && child.Delete(x, y))
                {
                    return true;
                }
            }
            return false;
        }

        public override Rectangle Find(int x, int y)
        {
            foreach (var child in Children)
            {
                if (child != null)
                {
                    var result = child.Find(x, y);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public override void Dump(int level)
        {
            foreach (var child in Children)
            {
                child?.Dump(level + 1);
            }
        }
    }
    public class Quadtree
    {
        private Node root;

        public Quadtree()
        {
            root = new LeafNode(-50, 50, -50, 50);
        }

        public bool Insert(Rectangle rect)
        {
            return root.Insert(rect);
        }

        public bool Delete(int x, int y)
        {
            return root.Delete(x, y);
        }

        public Rectangle Find(int x, int y)
        {
            return root.Find(x, y);
        }

        public void Dump()
        {
            root.Dump(0);
        }
    }
    public class CommandProcessor
    {
        private Quadtree quadtree;

        public CommandProcessor(string filename)
        {
            quadtree = new Quadtree();
            ProcessCommands(filename);
        }

        private void ProcessCommands(string filename)
        {
            var lines = File.ReadAllLines(filename);
            foreach (var line in lines)
            {
                var parts = line.Split(' ');
                var command = parts[0];

                switch (command.ToLower())
                {
                    case "insert":
                        int x = int.Parse(parts[1]);
                        int y = int.Parse(parts[2]);
                        int length = int.Parse(parts[3]);
                        int width = int.Parse(parts[4].Trim(';'));
                        if (!quadtree.Insert(new Rectangle(x, y, length, width)))
                        {
                            Console.WriteLine($"You cannot double insert at {x}, {y}.");
                        }
                        break;
                    case "find":
                        x = int.Parse(parts[1]);
                        y = int.Parse(parts[2].Trim(';'));
                        var rect = quadtree.Find(x, y);
                        if (rect == null)
                        {
                            Console.WriteLine($"Nothing is at {x}, {y}.");
                        }
                        else
                        {
                            Console.WriteLine(rect);
                        }
                        break;
                    case "delete":
                        x = int.Parse(parts[1]);
                        y = int.Parse(parts[2].Trim(';'));
                        if (!quadtree.Delete(x, y))
                        {
                            Console.WriteLine($"Nothing to delete at {x}, {y}.");
                        }
                        break;
                    case "update":
                        x = int.Parse(parts[1]);
                        y = int.Parse(parts[2]);
                        length = int.Parse(parts[3]);
                        width = int.Parse(parts[4].Trim(';'));
                        rect = quadtree.Find(x, y);
                        if (rect == null)
                        {
                            Console.WriteLine($"Nothing to update at {x}, {y}.");
                        }
                        else
                        {
                            rect.Length = length;
                            rect.Width = width;
                        }
                        break;
                    case "dump":
                        quadtree.Dump();
                        break;
                }
            }
        }
    }

}
