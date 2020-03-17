using System;
using System.Collections.Generic;

namespace BunnyWorld
{
	// The bunny object class.
	class Bunny
	{
		public string sex;
		public string color;
		public int age;
		public string name;
		public string house;

		public Bunny(string sex, string color, int age, string name, string house)
		{
			this.sex = sex;
			this.color = color;
			this.age = age;
			this.name = name;
			this.house = house;
		}

		public void PrintBunnyData()
		{
			Console.WriteLine("Sex: {0}, Color: {1}, Age: {2}, Name: {3}, House: {4}", sex, color, age, name, house);
		}
	}

	class Program
	{
		enum Colors
		{
			White,
			Brown,
			Grey,
			Black,
			Gold,
			Silver
		};

		enum Houses
		{
			Stark,
			Baratheon,
			Lannister,
			Targaryen
		};

		static void Main(string[] args)
		{
			Console.WriteLine("Here are the bunnies:");

			LinkedList<Bunny> bunnies = new LinkedList<Bunny>();

			// Inializing the list, creating 5 bunnies.
			for (int i = 0; i < 5; i++)
			{
				string sex;
				if (new Random().NextDouble() < 0.5)
					sex = "Male";
				else
					sex = "Female";

				string color;
				string house;
				if (new Random().NextDouble() < 0.98)
				{
					color = Enum.GetName(typeof(Colors), new Random().Next(6));
					house = Enum.GetName(typeof(Houses), new Random().Next(4));
				}
				else
				{
					color = "White";
					house = "White Walker";
				}

				Bunny bunny = new Bunny(sex, color, 0, "b" + (i + 1).ToString(), house);
				bunnies.AddLast(bunny);
				PrintANewbornBunny(bunny);
			}

			/* Listen for click events to move forward to the next turn.
			 * Terminate the program when all the bunnies have died.
			 */
			Console.WriteLine("Press any key for the next turn. Press ESC to stop.");
			while (Console.ReadKey(true).Key != ConsoleKey.Escape && bunnies.Count > 0)
			{
				if (!Console.KeyAvailable)
				{
					NextTurn(bunnies);
				}
			};
		}

		// Performing a turn and making modifications.
		private static void NextTurn(LinkedList<Bunny> bunnies)
		{
			LinkedList<Bunny> deadBunnies = new LinkedList<Bunny>();
			LinkedList<Bunny> newbornBunnies = new LinkedList<Bunny>();

			foreach (Bunny bunny in bunnies)
			{
				bunny.age++;

				/* A bunny dies when he becomes older than 10 years old.
				 * A White Walker bunny dies when he becomes 50 years old.
				 */
				if ((bunny.house != "White Walker" && bunny.age > 10) || (bunny.house == "White Walker" && bunny.age >= 50))
					deadBunnies.AddLast(bunny);
			}

			// Removing the dead bunnies from the list (needs to happen before breeding, to prevent a dead father).
			foreach (Bunny deadBunny in deadBunnies)
			{
				bunnies.Remove(deadBunny);
				PrintADeadBunny(deadBunny);
			}

			// Creating another foreach loop, because we need to age all the bunnies before we start the breeding.
			foreach (Bunny bunny in bunnies)
			{ 
				// Mating a female bunny with a male bunny.
				if(bunny.house != "White Walker" && bunny.sex == "Female" && bunny.age >=2)
				{
					Bunny adultMaleBunny = FindAnAdultMale(bunnies);
					if(adultMaleBunny != null)
					{
						// Create a new baby bunny.
						string sex;
						if (new Random().NextDouble() < 0.5)
							sex = "Male";
						else
							sex = "Female";

						string color;
						string house;
						if (new Random().NextDouble() < 0.98)
						{
							color = bunny.color;
							house = adultMaleBunny.house;
						}
						else
						{
							color = "White";
							house = "White Walker";
						}

						newbornBunnies.AddLast(new Bunny(sex, color, 0, "b" + (bunnies.Count + newbornBunnies.Count + 1).ToString(), house));
					}
				}
			}

			// Adding the newborn bunnies to the list.
			foreach(Bunny newBornBunny in newbornBunnies)
			{
				bunnies.AddLast(newBornBunny);
				PrintANewbornBunny(newBornBunny);
			}


			if (bunnies.Count > 1000)
			{
				// Kill half the population randomly.
			}

			// PrintBunnies(bunnies);
			Console.WriteLine("Press any key for next turn. Press ESC to stop.");
		}

		// Finding a random adult male bunny for mating.
		private static Bunny FindAnAdultMale(LinkedList<Bunny> bunnies)
		{
			LinkedList<Bunny> adultMaleBunnies = new LinkedList<Bunny>();

			foreach (Bunny bunny in bunnies)
			{
				if (bunny.house != "White Walker" && bunny.sex == "Male" && bunny.age >= 2)
					adultMaleBunnies.AddLast(bunny);
			}

			// Creating a random index to get a randomized adult male bunny.
			int selectedMaleBunnyIndex = new Random().Next(adultMaleBunnies.Count);
			foreach(Bunny adultMaleBunny in adultMaleBunnies)
			{
				if (selectedMaleBunnyIndex == 0)
					return adultMaleBunny;
				// Downgrading the index until we find the randomized bunny.
				selectedMaleBunnyIndex--;
			}
			return null;
		}

		// Print a message for each newborn bunny.
		private static void PrintANewbornBunny(Bunny bunny)
		{
			if(bunny.house != "White Walker")
			{
				if (bunny.sex == "Male")
					Console.WriteLine("Lord {0} of bunny house {1} of color {2} was born!", bunny.name, bunny.house, bunny.color);
				else
					Console.WriteLine("Lady {0} of bunny house {1} of color {2} was born!", bunny.name, bunny.house, bunny.color);
			}
			else
				Console.WriteLine("White Walker bunny {0} was born!", bunny.name);
		}

		// Print a message for each dead bunny.
		private static void PrintADeadBunny(Bunny bunny)
		{
			if (bunny.house != "White Walker")
			{
				string sigil;
				string saying;
				switch(bunny.house)
				{
					case "Stark":
						sigil = "Wolf";
						saying = "Winter Is Coming!";
						break;
					case "Baratheon":
						sigil = "Stag";
						saying = "Ours Is The Furry!";
						break;
					case "Lannister":
						sigil = "Lion";
						saying = "Hear Me Roar!";
						break;
					case "Targaryen":
						sigil = "Dragon";
						saying = "Fire And Blood!";
						break;
					default:
						sigil = "";
						saying = "";
						break;
				}
				if (bunny.sex == "Male")
					Console.WriteLine("Lord {0} of bunny house {1} died at age {2}! He was a fierce {3}! {4}", bunny.name, bunny.house, bunny.age, sigil, saying);
				else
					Console.WriteLine("Lady {0} of bunny house {1} died at age {2}! She was a fierce {3}! {4}", bunny.name, bunny.house, bunny.age, sigil, saying);
			}
			else
				Console.WriteLine("White Walker {0} died at age {1}!", bunny.name, bunny.age);
		}

		// Print the entire bunnies list.
		private static void PrintBunnies(LinkedList<Bunny> bunnies)
		{
			foreach (Bunny bunny in bunnies)
			{
				bunny.PrintBunnyData();
			}
		}
	}
}
