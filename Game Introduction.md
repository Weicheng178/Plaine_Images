#Projet Plaine_Images


1.Introduction

Design mode: singleton mode, each client will only be assigned one server or client, there can only be one server and multiple clients
MVC: I used the mvc architecture, but if I have to say it, M is the data of the application layer, v is the xaml design file, and c is the delegate of the application layer
Synchronous mechanism: using asynchronous socket to accept data, and delegate to update UI

2.Game Rules

For the host player, first specify the number of player and rounds, then click create room to host the game
For the players, when host player click start round, a question will occur, player will have to decide whether to answer the question by clicking yes/no, 
if yes a answer must be supplied, if the players have a right guess, 2 marks will be given, if no player have the right answer, answers that closes to the right answer by 1 will get 1 mark
after the specified rounds, the highest mark player will have the you win popup, and the game ends
