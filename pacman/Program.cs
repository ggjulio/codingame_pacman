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
			game.SyncGame();
			game.Play();
        }
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
	}

	public void Update(Vector2 position, string typeId, int speedTurnsLeft, int abilityCooldown)
	{
		base.Update(position);
		Positions.Add(position);
		this.TypeId = typeId;
		this.SpeedTurnsLeft = speedTurnsLeft;
		this.AbilityCooldown = abilityCooldown;
	}

	public void Move(Vector2 targetPosition)
	{
		Console.WriteLine($"MOVE {this.Id} {targetPosition.X} {targetPosition.Y}");
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
}

public class Cell
{
	public bool   IsWall{get;}
	public bool   IsFloor{get;}
	public Entity Inside{get; set;}	
	public Cell(char cell)
	{
		IsFloor = cell == ' ' ? true : false;
		IsWall = !IsFloor;
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

		this.Map = new Cell[Height, Width];
		
        for (int i = 0; i < Height; i++)
        {
			// one line of the grid: space " " is floor, pound "#" is wall
            string row = Console.ReadLine();
		    for (int j = 0; j < Width; j++)
				Map[i,j] = new Cell(row[j]);
        }

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

	public void SyncGame()
	{
		string[] inputs = Console.ReadLine().Split(' ');
		
		
		this.MyScore = int.Parse(inputs[0]);
		this.OpponentScore = int.Parse(inputs[1]);
		this.VisiblePacCount = int.Parse(Console.ReadLine()); // all your pacs and enemy pacs in sight
		
		/// LOOP PACS
		for (int i = 0; i < this.VisiblePacCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');
			
			int pacId = int.Parse(inputs[0]); // pac number (unique within a team)
			bool mine = inputs[1] != "0"; // true if this pac is yours
			Vector2 position = new Vector2(int.Parse(inputs[2]), int.Parse(inputs[3])); // position in the grid
			string typeId = inputs[4]; // unused in wood leagues
			int speedTurnsLeft = int.Parse(inputs[5]); // unused in wood leagues
			int abilityCooldown = int.Parse(inputs[6]); // unused in wood leagues

			Pac pac = Pacs.Find(e => e.Id == pacId);
			if (pac == null)
				this.Pacs.Add(new Pac(pacId, mine, position, typeId, speedTurnsLeft, abilityCooldown));
			else
				pac.Update(position, typeId, speedTurnsLeft, abilityCooldown);	
		}

		this.VisiblePelletCount = int.Parse(Console.ReadLine()); // all pellets in sight
		
		// LOOP PELLETS
		for (int i = 0; i < this.VisiblePelletCount; i++)
		{
			inputs = Console.ReadLine().Split(' ');

			Vector2 position = new Vector2(int.Parse(inputs[0]), int.Parse(inputs[1]));
			int value = int.Parse(inputs[2]); // amount of points this pellet is worth
			this.Pellets = new List<Pellet>(this.VisiblePelletCount);
			this.Pellets.Add(new Pellet(position, value));
		}

	}
	public void Play()
	{
            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            Console.WriteLine("MOVE 0 15 10"); // MOVE <pacId> <x> <y>
	}
}