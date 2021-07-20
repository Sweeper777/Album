# Album Language Specification

Album source code is written like a playlist, in the form of a list of songs (instructions). Each song, when played (executed), does something different to the stack. Everything is case insensitive. The stack has an implementation-dependent size and stores signed 32-bit integers. When an empty stack is popped, or when an element is pushed onto a full stack, the playlist should terminate.

An Album source file begins with `Playlist created by <name>`, where the creator of the playlist (program) is declared. Every line before this line is a comment, and every line of this form after the first such line is also a comment.

After declaring the creator of the playlist, one of the following songs can be written on each line, otherwise it is considered a comment:

    VORACITY, by Myth & Roid

Pushes the integer value of the user input, encoded in an implementation-dependent encoding that must be compatible with ASCII, onto the stack.

    Do you hear the people sing? by Les Miserables

Pops the stack, output that element as an ASCII character. If the number is out of the range of ASCII, the behaviour is implementation-dependent.

    Gasoline, by Halsey

Pops the stack, output that element as a number.

    Man in the Mirror, by Michael Jackson

Pops the stack, pushes that element multiplied by 2.

    The Right Path, by Thomas Greenberg

Pops the stack, pushes that element after an arithmetic right shift by 1.

    Killer Queen, by Queen

Clears the stack.

    Let It Go, by Idina Menzel

Pops the stack.

    Dirty Deeds Done Dirt Cheap, by AC/DC

Pops the stack, then pushes that element twice.

    Dear Maria, Count Me In, by All Time Low

Pops the stack twice, then pushes the sum of the elements.

    Sink or Swim, by Tyrone Wells

Pops the stack twice, then pushes the bitwise OR of the elements.

    Take it from me, by The Weepies

Pops the stack twice, then pushes the result of the second popped element minus the first popped element.

    Hideaway, by The Weepies

Pops the stack twice, then pushes the bitwise AND of the elements.

    Desperado, by The Eagles

Pops the stack twice, then pushes the bitwise exclusive OR of the elements.

    I'm so Tired, by Lauv & Troye Sivan

Stops playing the playlist (terminates the program).

    Zenzenzense, by RADWIMPS

Pops the stack twice, then pushes the first popped element, and the second popped element, effectively swapping the top two elements on the stack.

    Roundabout, by YES

Removes the bottom element of the stack, then pushes it onto the stack.

    Rolling in the Deep, by Adele

Pops the stack, adds it to the stack as the bottom element.

    King Nothing, by Metallica

Pops the stack, if the popped element is 0, push 1, otherwise push 0.

    LOSER, by Kenshi Yonezu

Pops the stack, if the popped element is less than 0, push 1, otherwise push 0.

    Nothing Compares 2U, by Sinead O'Connor

Pops the stack, if the popped element is greater than 0, push 1, otherwise push 0.

    Never Gonna Give You Up, by Rick Astley

This song always loops, hence creating an infinite loop.

    <number> Bottles of Beer On The Wall

Pushes `<number>` onto the stack. `<number>` can only be non-negative, and less than 100, because 100 beers is too many beers. The wall can't possibly handle that.

    <original song name>, by <playlist creator name>

This declares an original song (label) if `<playlist creator name>` is the same as the name in the playlist creator declaration.

    Country Roads, Take Me <original song name>

Pops the stack, the playlist will start playing from the specified original song (jump to that label), if the popped element is not 0.

There are also songs that push specific numbers onto the stack:

| Song Name                                                     | Number Pushed |
|---------------------------------------------------------------|---------------|
| 50 Ways to Say Goodbye, by Train                              | 50            |
| Senbonzakura, by Kurousa-P                                    | 1000          |
| I'm Gonna Be (500 Miles), by The Proclaimers                  | 500           |

Alternative spellings of all of these song names could be accepted depending on the implementation.

Each song name can have trailing and leading whitespace, and can optionally be ended with a semicolon, or a period.

An Album implementation is NOT required to terminate the playlist after the last song is played, so it is recommended to add the song "I'm So Tired, by Lauv & Troye Sivan" at the end of every playlist that isn't intended to play on loop forever.