# Reversi
Reversi written in Unity

The game is a 1 player game against an AI. The player can select a color (black always goes first) and set the difficulty for the AI from 1 to 10. This difficulty corresponds to how many moves ahead the AI will look before making a move.

### Features
- Legal move highlighting. No need to "guess" which moves are legal or not, and this makes it far easier for the human player to strategize.
- Human vs. AI by default, with difficulty ranging from 1-10. If desirable, human vs. human can be played by editing `GameController.AITurn()` and following the instructions within the function.
- Background music composed and performed by me. The song is also on [my SoundCloud](https://soundcloud.com/ajbowler/the-4-9-evening-shift).


### Known Issues
- Sometimes the rotations behave funnily such that no rotations are performed when the pieces are captured. This is a bug I have not been able to track down.
