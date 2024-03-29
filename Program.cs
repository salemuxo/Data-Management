﻿using System.Text.Json;

public static class MusicLibrary
{
    public static List<Album> albums;
    public static List<User> users;
    public static int currentUserIndex;

    public static void Main(string[] args)
    {
        // clear console and load data
        Console.Clear();
        albums = LoadJsonData<Album>("album-data");
        users = LoadJsonData<User>("users-data");

        // login loop
        bool loggingIn = true;
        while (loggingIn)
        {
            string menuSel = WriteLoginMenu();
            Console.WriteLine();
            switch (menuSel)
            {
                case "1": // login
                {
                    string username = ConsolePrompt("Enter username");
                    string password = ConsolePrompt("Enter password");

                    User matchingUser = users.Find(x => x.Username == username);
                    if (matchingUser == null || matchingUser.Password != password)
                    {
                        Console.WriteLine("Invalid username / password");
                    }
                    else
                    {
                        currentUserIndex = users.FindIndex(x => x == matchingUser);
                        Console.WriteLine("Logged in successfully");
                        loggingIn = false;
                    }

                    break;
                }
                case "2": // sign up
                {
                    string username = ConsolePrompt("Enter username");
                    string password = ConsolePrompt("Enter password");

                    User matchingUser = users.Find(x => x.Username == username);
                    if (matchingUser != null)
                    {
                        Console.WriteLine("User already exists");
                    }
                    else
                    {
                        users.Add(new User(username, password));
                        Console.WriteLine("Signed up successfully! Please log in.");
                    }

                    break;
                }
                default:
                {
                    Console.WriteLine("Invalid input");
                    break;
                }
            }
            Console.WriteLine();
        }

        Console.WriteLine("MUSIC LIBRARY");

        // main loop
        bool isRunning = true;
        while (isRunning)
        {
            string menuSel = WriteMenu();
            Console.WriteLine();

            switch (menuSel)
            {
                case "1": // display all
                {
                    Console.WriteLine("ALL ALBUMS");
                    WriteAlbumsInList(albums);
                    break;
                }
                case "2": // filter by property
                {
                    string propertyName = GetPropertySelection();
                    string filterString = ConsolePrompt("Enter value to filter for");

                    List<Album> filteredAlbums = new List<Album>();
                    // filter for genre
                    if (propertyName == "Genres")
                    {
                        foreach (Album album in albums)
                        {
                            // find matching genre's index
                            int matchingIndex = Array.FindIndex(
                                album.Genres,
                                x => x.ToLower() == filterString.ToLower()
                            );
                            if (matchingIndex == -1) // if no matching genre, continue
                            {
                                continue;
                            }
                            else if (matchingIndex != 0) // if matching genre isnt first, make it first
                            {
                                SwapValues<string>(album.Genres, 0, matchingIndex);
                            }
                            filteredAlbums.Add(album);
                        }
                    }
                    else // filter for anything else
                    {
                        // adds all elements in album that match filterString to filteredAlbums
                        filteredAlbums = albums.FindAll(
                            x =>
                                typeof(Album)
                                    .GetProperty(propertyName)
                                    .GetValue(x)
                                    .ToString()
                                    .ToLower() == filterString.ToLower()
                        );
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Albums with {propertyName} matching {filterString}: ");
                    WriteAlbumsInList(filteredAlbums, albums);
                    break;
                }
                case "3": // sort by property
                {
                    string propertyName = GetPropertySelection();

                    // time sort
                    decimal startTime = GetCurrentTime();
                    albums = SortBy(albums, propertyName);
                    decimal endTime = GetCurrentTime();
                    Console.WriteLine($"Sorted by {propertyName} ({endTime - startTime} seconds)");
                    break;
                }
                case "4": // add to favourites
                {
                    string favouriteIndexStr = ConsolePrompt(
                        "Enter index of album to add to favourites"
                    );
                    if (
                        Int32.TryParse(favouriteIndexStr, out int favouriteIndex)
                        && favouriteIndex < albums.Count
                    )
                    {
                        users[currentUserIndex].Favourites.Add(albums[favouriteIndex]);
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input");
                    }
                    break;
                }
                case "5": // remove from favourites
                {
                    string favouriteIndexStr = ConsolePrompt(
                        "Enter index of album to remove from favourites"
                    );
                    if (
                        Int32.TryParse(favouriteIndexStr, out int favouriteIndex)
                        && favouriteIndex < users[currentUserIndex].Favourites.Count
                    )
                    {
                        users[currentUserIndex].Favourites.RemoveAt(favouriteIndex);
                    }
                    else
                    {
                        Console.WriteLine("Invalid Input");
                    }
                    break;
                }
                case "6": // display favourites
                {
                    Console.WriteLine("FAVOURITE ALBUMS");
                    WriteAlbumsInList(users[currentUserIndex].Favourites);
                    break;
                }
                case "7": // configure settings
                {
                    string showGenre = ConsolePrompt("Would you like to show album's genres? (Y/N)")
                        .ToLower();
                    string showYear = ConsolePrompt("Would you like to show album's year? (Y/N)")
                        .ToLower();
                    Settings.showGenre = (showGenre == "y");
                    Settings.showYear = (showYear == "y");
                    break;
                }
                case "8": // exit
                {
                    isRunning = false;
                    break;
                }
                default:
                {
                    Console.WriteLine("Invalid input");
                    break;
                }
            }
        }
        SaveJsonData(albums, "album-data");
        SaveJsonData(users, "users-data");
    }

    // write selection menu and return selection
    static string WriteMenu()
    {
        Console.WriteLine();
        Console.WriteLine("LIBRARY MENU");
        Console.WriteLine("1: Display All");
        Console.WriteLine("2: Filter by Property");
        Console.WriteLine("3: Sort by Property");
        Console.WriteLine("4: Add to Favourites");
        Console.WriteLine("5: Remove from Favourites");
        Console.WriteLine("6: Display Favourites");
        Console.WriteLine("7: Configure Settings");
        Console.WriteLine("8: Exit and Save");
        return ConsolePrompt("Enter a selection (1-8)");
    }

    // write login menu and return selection
    static string WriteLoginMenu()
    {
        Console.WriteLine("LOGIN / SIGNUP");
        Console.WriteLine("1: Login");
        Console.WriteLine("2: Sign Up");
        return ConsolePrompt("Enter a selection (1-2)");
    }

    // load album data from fileName.json, return list
    static List<T> LoadJsonData<T>(string fileName)
    {
        string jsonString = File.ReadAllText($"{fileName}.json");
        return JsonSerializer.Deserialize<List<T>>(jsonString);
    }

    // save album data from list to fileName.json
    static void SaveJsonData<T>(List<T> list, string fileName)
    {
        string jsonString = JsonSerializer.Serialize(list);
        File.WriteAllText($"{fileName}.json", jsonString);
    }

    // write prompText: and return response
    static string ConsolePrompt(string promptText)
    {
        Console.Write($"{promptText}: ");
        return Console.ReadLine();
    }

    // sort list by property name, returns list
    static List<Album> SortBy(List<Album> toSort, string propertyName)
    {
        if (typeof(Album).GetProperty(propertyName) == null)
        {
            return null;
        }
        else if (propertyName == "Genres")
        {
            return toSort.OrderBy(album => album.Genres[0]).ToList();
        }
        else
        {
            return toSort
                .OrderBy(album => typeof(Album).GetProperty(propertyName).GetValue(album))
                .ToList();
        }
    }

    // return name of property as string from selection
    static string GetPropertySelection()
    {
        Console.WriteLine("Available properties: 1 - Title, 2 - Artist, 3 - Year, 4 - Genre");

        string propertyInput = ConsolePrompt("Enter property to sort by");
        string propertyName;
        switch (propertyInput)
        {
            case "1":
                propertyName = "Title";
                break;
            case "2":
                propertyName = "Artist";
                break;
            case "3":
                propertyName = "Year";
                break;
            case "4":
                propertyName = "Genres";
                break;
            default:
                propertyName = null;
                break;
        }
        return propertyName;
    }

    // return current time in seconds
    static decimal GetCurrentTime()
    {
        decimal timeInSeconds = DateTime.Now.Ticks / (decimal)TimeSpan.TicksPerSecond;
        return timeInSeconds;
    }

    // swap values in array
    static void SwapValues<T>(this T[] source, long index1, long index2)
    {
        T temp = source[index1];
        source[index1] = source[index2];
        source[index2] = temp;
    }

    // write albums in list to console, index from same list
    static void WriteAlbumsInList(List<Album> aList)
    {
        for (int i = 0; i < aList.Count; i++)
        {
            Console.WriteLine($"{i}: {aList[i].ToString()}");
        }
    }

    // write albums in lsit to console, index from other list
    static void WriteAlbumsInList(List<Album> aList, List<Album> indexList)
    {
        foreach (Album album in aList)
        {
            Console.WriteLine($"{indexList.FindIndex(x => x == album)}: {album.ToString()}");
        }
    }
}

public class Album
{
    public string Title { get; set; }
    public string Artist { get; set; }
    public int Year { get; set; }
    public string[] Genres { get; set; }

    public Album(string title, string artist, int year, params string[] genres)
    {
        this.Title = title.Trim();
        this.Artist = artist.Trim();
        this.Year = year;
        this.Genres = genres;
    }

    public override string ToString()
    {
        string albumString = $"{Artist} - {Title}";
        if (Settings.showYear)
        {
            albumString += $" ({Year})";
        }
        if (Settings.showGenre)
        {
            albumString += $" [{String.Join(", ", Genres)}]";
        }

        return albumString;
    }
}

public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    public List<Album> Favourites { get; set; }

    public User(string username, string password)
    {
        this.Username = username;
        this.Password = password;
        this.Favourites = new List<Album>();
    }

    public override string ToString()
    {
        return $"{Username} - {Password}";
    }
}

public static class Settings
{
    public static bool showGenre = true;
    public static bool showYear = true;
}
