Fortis Code Challenge - five_cards_poker

Prompt:
Write a backend application in C# that can output five-card poker hands and evaluate which hand is the highest. The rules for evaluating poker hands can be found here:
https://en.wikipedia.org/wiki/List_of_poker_hands

Notes on requirements:
● The application only has to handle cards from a standard 52-card deck, without any wild cards or jokers.
● This exercise is intentionally ambiguous; you are free to design the API and its endpoints.
● You can decide how the application handles ties and invalid input.

Description:
This project is a backend application written in C#. Its objective is to simulate the generation of poker hands from a 52-card deck, validate them, and determine a winning hand.

The project was created with two controllers:

1. PokerController
This controller simulates a complete game round. The only mandatory input is the number of players. All hands are generated internally, evaluated, and the final result is a summary of the round (indicating which player won and with which cards). A "shuffleSeed" input was added to allow replaying the same game for debugging purposes.

2. PokerActionsController
This controller separates the main actions performed by the PokerController into individual endpoints. This allows them to be called independently when needed. It contains multiple endpoints with the same validation logic, supporting both single-hand and multiple-hand evaluation.

The ReturnTableResult... action has two versions:
    One expects a PlayerDTO object (used for the internal/controlled process).
    Another expects a List of List of Cards, allowing it to be called directly after the GetHand endpoint for fast game validation.

Both versions perform the same validation but expect different data inputs.

Architecture:
Each controller has its own dedicated Service file. The PokerService handles the internal simulation and, because it controls the entire process, uses fewer input validations and more optimized loops for hand evaluation. In contrast, the PokerActionsService requires more robust validation as it handles external requests and performs actions separately. For this reason, I preferred to keep the two approaches distinct. Both services utilize the same Repository.

Instructions:
    Open the project in an IDE (Visual Studio is recommended).
    Build the project.
    Run it.