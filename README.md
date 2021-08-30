# Album

Album is a stack-based, Turing-complete, esoteric programming language inspired by [Super Stack!](https://esolangs.org/wiki/Super_Stack!).

Album programs are called "playlists". Each playlist starts with a declaration saying by whom this playlist was created, followed by a list of songs, which act as instructions and branch labels. See [spec.md](spec.md) for the language specification and the list of built-in songs.

For some example programs written in Album, see the [`Resources/Examples`](Resources/Examples) folder.

## The C# Implementation

This repo implements an Album compiler that outputs CIL, written in C# 9 and .NET 5.

### Testing

    dotnet test

### Building From Source

    dotnet build

This creates an `Album.dll` executable at the `Album/bin/Debug/net5.0` directory. For more options, see the [documentation](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build). To run it, use the `dotnet` command:

    dotnet Album/bin/Debug/net5.0/Album.dll <INPUT SOURCE FILE> [options]

You can also choose to deploy the Album compiler using [`dotnet publish`](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish).

### Compiler Options

```none
-o, --output        Path to output file.

-P, --parse-only    Generates the parser output, with optimisations applied,
                    if any

-R, --run           Runs the program immediately, without generating any
                    files.

-O, --optimise      Smaller output code size, longer compile time

-w, --warn          (Default: Warning) What warnings should be output as Valid
                    values: None, Warning, Error

-s, --manifest      Path to custom song manifest file

--help              Display this help screen.
```

If `-o` is not specified, the default output file path is `./Program.exe`, or `./ParserOutput.txt` in the case of `-P`. In case an executable is generated, an additional `.runtimeconfig.json` file with the same name as the executable will be generated in the same directory.

## Song Manifests

In case you don't like my choice of songs, you can make up your own songs, write them in a JSON file, and pass that to the compiler when compiling your code. This is known as a song manifest. The default song manifest is located in `Resources/`. You can use that as an example when creating your own song manifests.

## Benchmarks

You can go into the `Album.Benchmarks` directory and run:

    dotnet run -c Release

to run the benchmarks. I compare the difference in compilation time when applying optimisation versus not doing that, and the execution time of optimised vs unoptimised Album code vs C# code. On my MacBook Pro, Album runs 30 times slower than C# for the 99 bottles of beer playlist.
