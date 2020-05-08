using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;


/**
 * Grab the pellets as fast as you can!
 **/
class Player
{
    static void Main(string[] args)
    {
		Game game = new Game();

        while (true)
        {
			game.Sync();
			game.Play();
        }
    }
}

public abstract class Entity
{
	public Vector2 Position{get; set;}

	public Entity(Vector2 position)
	{
		this.Position = position;
	}

	public void Update(Vector2 position)
	{
		this.Position = position;
	}
	public float Distance(Entity e)
	{
		return(Vector2.Distance(Position, e.Position));
	}
	public float Distance(Vector2 v)
	{
		return(Vector2.Distance(Position, v));
	}
	public override string ToString()
	{
		return ($"Entity(Position:{Position})");
	}
}

public class Pac : Entity
{
	public int Id{get;}
	public bool Mine{get;}
	public List<Vector2> Positions{get;set;}
	public string TypeId{get;set;}
	public int SpeedTurnsLeft{get;set;}
	public int AbilityCooldown{get;set;}

	public Pellet TargetPellet{get;set;}
	public bool	IsAlive{get;set;}

	public Pac(int id, bool mine, Vector2 position, string typeId,
		int speedTurnsLeft, int abilityCooldown) : base(position)
	{
		this.Id = id;
		this.Mine = mine;
		this.Positions = new List<Vector2>();
		this.Positions.Add(position);
		this.TypeId = typeId;
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = true;
	}

	public void Update(Vector2 position, string typeId, int speedTurnsLeft, int abilityCooldown)
	{
		base.Update(position);
		Positions.Add(position);
		this.TypeId = typeId;
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.AbilityCooldown = abilityCooldown;
		this.IsAlive = true;
	}

	public void Move(Vector2 targetPosition)
	{
		Console.Write($"MOVE {this.Id} {targetPosition.X} {targetPosition.Y}|");
	}
	public override string ToString()
	{
		return $"Pac(Id:{Id};Mine:{Mine};Position:{Position};SpeedTurnsLeft:{SpeedTurnsLeft};AbilityCooldown:{AbilityCooldown})";
	}
}
public class Pellet : Entity
{
	public int Value {get;}

	public Pellet(Vector2 position, int value) : base(position)
	{
		this.Value = value;
	}
}

public class Cell
{
	public bool   	IsWalkable{get;}
	public bool   	IsVisiblePellet{get;set;}
	public bool	  	HasPellet{get;set;}
	public Entity 	Inside{get; set;}	
	public Vector2	Position{get;}
	public Cell(char cell, Vector2 position)
	{
		this.IsWalkable = cell == ' ' ? true : false;
		if (IsWalkable)
			this.HasPellet = true;
		this.IsVisiblePellet = false;
		this.Position = position;
	}
}
public class Grid
{
	public int Height{get; set;}
	public int Width{get; set;}
	public Cell[,] Map{get; set;}

	public Grid(int width, int height)
	{
		this.Width = width;
		this.Height = height;

		this.Map = new Cell[width, Height];
		
        for (int i = 0; i < Height; i++)
        {
			// one line of the grid: space " " is floor, pound "#" is wall
            string row = Console.ReadLine();
		    for (int j = 0; j < Width; j++)
				Map[j,i] = new Cell(row[j], new Vector2(j, i));
        }
	}
	public void Update(List<Pac> pacs, List<Pellet> pellets)
	{
		foreach (Cell c in Map)
		{
			c.Inside = null;
			c.IsVisiblePellet = false;
		}
		foreach (Pac p in pacs.Where(e => e.Mine).ToList())
		{
			for (int i = 1; p.Position.X + i < this.Width && Map[(int)p.Position.X + i, (int)p.Position.Y].IsWalkable; i++)
				Map[(int)p.Position.X + i, (int)p.Position.Y].HasPellet = false;
			for (int i = -1; p.Position.X + i > 0 && Map[(int)p.Position.X + i, (int)p.Position.Y].IsWalkable; i--)
				Map[(int)p.Position.X + i, (int)p.Position.Y].HasPellet = false;
		
			for (int i = 1; p.Position.Y + i < this.Height && Map[(int)p.Position.X, (int)p.Position.Y + i].IsWalkable; i++)
				Map[(int)p.Position.X, (int)p.Position.Y + i].HasPellet = false;
			for (int i = -1; p.Position.Y + i > 0 && Map[(int)p.Position.X, (int)p.Position.Y + i].IsWalkable; i--)
				Map[(int)p.Position.X, (int)p.Position.Y + i].HasPellet = false;
		}

		foreach (Pellet p in pellets)
		{
			Map[(int)p.Position.X, (int)p.Position.Y].Inside = p;
			Map[(int)p.Position.X, (int)p.Position.Y].HasPellet = true;
			Map[(int)p.Position.X, (int)p.Position.Y].IsVisiblePellet = true;

		}
		foreach (Pac p in pacs)
		{
			Map[(int)p.Position.X, (int)p.Position.Y].Inside = p;
			Map[(int)p.Position.X, (int)p.Position.Y].HasPellet = false;

		}
	}

	public override string ToString()
	{
		string result = "";

		for (int i = 0; i < this.Height; i++)
		{
			for (int j = 0; j < this.Width; j++)
			{
				// if (this.Map[j, i].Inside != null)
				// {
				// 	if (this.Map[j,i].Inside is Pac pa)
				// 		result += pa.Id;
				// 	else if (this.Map[j, i].Inside is Pellet pe)
				// 		result += "-";
				// 	else
				// 		result += "!";
				// }
				// else
				// 	result += this.Map[j, i].IsWalkable ? " " : "#";

///	/////////////////////////////////////////////////////////////////

				if (this.Map[j,i].HasPellet)
						result += "-";
				else if (this.Map[j, i].Inside != null)
				{
					if (this.Map[j,i].Inside is Pac pa)
						result += pa.Id;
					else
						result += "!";
				}
				else
					result += this.Map[j, i].IsWalkable ? " " : "#";			
			}
			result += "\n";
		}
		return result;
	}
}

public class Game{
	public Grid Grid{get;set;}
	public int MyScore {get; set;}
	public int OpponentScore {get; set;}
	public int VisiblePacCount {get; set;}
	public List<Pac> Pacs{get;set;}
	public int VisiblePelletCount {get; set;}
	public List<Pellet> Pellets{get;set;}

	public Game()
	{
		string[] inputs = Console.ReadLine().Split(' ');

        int width = int.Parse(inputs[0]); // size of the grid
        int height = int.Parse(inputs[1]); // top left corner is (x=0, y=0)

		this.Grid = new Grid(width,height);
		this.Pacs = new List<Pac>(10); // 10 pacman max (5 by team)
	}

	public void Sync()
	{
		string[] inputs = Console.ReadLine().Split(' ');
		
		
		this.MyScore = int.Parse(inputs[0]);
		this.OpponentScore = int.Parse(inputs[1]);
		
		// Reset is alive to false foreach pac
		foreach (Pac p in GetMyPacs())
			p.IsAlive = false;
		/// LOOP PACS
		this.VisiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight		

		for (int i = 0; i < this.VisiblePacCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
			bool mine = inputs[1] != "0"; // true if this pac is yours
			Vector2 position = new Vector2(int.Parse(inputs[2]), int.Parse(inputs[3])); // position in the grid
			string typeId = inputs[4]; // unused in wood leagues
			int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
			int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

			Pac pac = Pacs.Find(e => e.Id == pacId && e.Mine == mine);
			if (pac == null)
				this.Pacs.Add(new Pac(pacId, mine, position, typeId, speedTurnsLeft, abilityCooldown));
			else
				pac.Update(position, typeId, speedTurnsLeft, abilityCooldown);	
		}

		// LOOP PELLETS
		this.VisiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
		this.Pellets = new List<Pellet>(this.VisiblePelletCount);

		for (int i = 0; i < this.VisiblePelletCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');

			Vector2 position = new Vector2(int.Parse(inputs[0]), int.Parse(inputs[1]));
			int value = int.Parse(inputs[2]); // amount of points this pellet is worth
			this.Pellets.Add(new Pellet(position, value));
		}
		this.Grid.Update(this.Pacs, this.Pellets);
	}


	public static void Debug(string message)
	{
		Console.Error.WriteLine(message);
	}

	public List<Pac> GetMyPacs()
	{
		return this.Pacs.Where(e => e.Mine && e.IsAlive).OrderBy(e => e.Id).ToList();
	}
	public List<Pac> GetOpponentPacs()
	{
		return this.Pacs.Where(e => !e.Mine).OrderBy(e => e.Id).ToList();
	}
	public List<Pellet> GetPelletsNearest(Entity p_e)
	{
		return this.Pellets.OrderBy(e => e.Distance(p_e)).ToList();
	}
	public void Play()
	{
		// reset target pellet if pellet not existing anymore
		foreach (Pac pa in GetMyPacs().Where(p => p.TargetPellet != null).ToList())
			if (!Pellets.Any(pe => pe.Position == pa.TargetPellet.Position))		
				pa.TargetPellet = null;

		// IF big pellets
		List<Pellet> bigPellets = Pellets.Where(p => p.Value == 10).ToList();
		foreach (Pellet pe in bigPellets)
		{
			Pac pa = GetMyPacs().OrderBy(p => p.Distance(pe)).First();

			if (pa.TargetPellet == null || pe.Distance(pa) < pa.TargetPellet.Distance(pa))
				pa.TargetPellet = pe;
		}
		Debug(Grid.ToString());
		foreach (Pac p in GetMyPacs())
		{
			if (p.TargetPellet == null)
			{
				List<Pellet>  AlreadyTargeted = GetMyPacs().Select(e => e.TargetPellet).Where(e => e != null).ToList();

				p.TargetPellet = GetPelletsNearest(p).Except(AlreadyTargeted).FirstOrDefault();
				
				if (p.TargetPellet == null)
					p.TargetPellet = GetPelletsNearest(p).FirstOrDefault();
			}

			if (p.TargetPellet != null)
				p.Move(p.TargetPellet.Position);
			else
			{
				Cell target = Grid.Map.Cast<Cell>().Where(e => e.HasPellet).OrderBy(e => p.Distance(e.Position)).FirstOrDefault();

				if (target != null)
					p.Move(target.Position);
				else
					p.Move(p.Position);
			}
		}
		Console.WriteLine();
	}
}