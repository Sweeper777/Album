# Album

Album is a stack-based, Turing-complete, esoteric programming language inspired by [Super Stack!](https://esolangs.org/wiki/Super_Stack!).

Album programs are called "playlists". Each playlist starts with a declaration saying by whom this playlist was created, followed by a list of songs, which act as instructions and branch labels. See [spec.md](spec.md) for the language specification and the list of built-in songs.

For some example programs written in Album, see the [`Resources/Examples`](Resources/Examples) folder.

## The C# Implementation

This repo implements an Album compiler that outputs CIL, written in C# 9 and .NET 5.

### Testing

    dotnet test

### Running

(The compiler has not been completed yet...)