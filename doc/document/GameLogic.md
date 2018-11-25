# Game Logic

After all **players** joined one game room and were ready for playing, the method implements the function providing the support the workflow below should be called.

![GameLogic](..\workflow\image\GameLogic.png)

<p style="text-align: center;"><b>Picture: The main logic of a game room.</b></p>

The `Main Game Logic` has three main sub-components, including the `Initializing` method, which will be called only once right after the `Main Game Logic` started and `Turn event` & `New-turn event`, which will be always called until the game ended, during handling these two `events`, the main workflow of the multiplayer-game will be executed and processed.