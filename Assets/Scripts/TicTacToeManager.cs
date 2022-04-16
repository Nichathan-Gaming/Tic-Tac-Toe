using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TicTacToeManager : MonoBehaviour
{
    [Header("The 3 possible sprites")]
    public Sprite clearSprite; 
    public Sprite xSprite; 
    public Sprite oSprite;

    public Text usesAIText;
    public Text aIgoesFirstText;
    public Dropdown difficultyLevelDropdown;

    [Header("The 9 board locations.")]
    public GameObject[] boardLocations;

    [SerializeField] private int[] boardMarks= new int[9];

    //use this for now
    private bool isO;

    //if false, do not allow move
    private bool gameActive=true;

    [Header("If true, player can only move on !aiIsX")]
    public bool usesAI;

    [Header("If true, the AI will move for X, else for O")]
    public bool aiIsX = true;

    [Header("AI difficulty: 0=easy")]
    public int aiDifficultyLevel = 0;

    //easy mode: pick random number 
    private List<int> openLocations = new List<int>();

    //the numbers that we use to check for victory
    private static int[,] checkNumbers = new int[8, 3]
    {
                { 0,1,2 },
                { 3,4,5 },
                { 6,7,8 },
                { 0,3,6 },
                { 1,4,7 },
                { 2,5,8 },
                { 0,4,8 },
                { 2,4,6 }
    };

    private int turn = 0;

    private int oFirstTurn = -1;

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResetGame()
    {
        turn = 0;
        gameActive = true;
        isO = false;
        boardMarks = new int[9];
        foreach(GameObject go in boardLocations)
        {
            go.GetComponent<Image>().sprite = clearSprite;
        }

        oFirstTurn = -1;

        openLocations.Clear();
        //initialize openLocations
        for (int i = 0; i < 9; i++)
        {
            openLocations.Add(i);
        }

        if (aiIsX && usesAI) ChooseAIMove();
    }

    public void ToggleUsesAI()
    {
        usesAI = !usesAI;

        usesAIText.text = usesAI ?
            "Uses Computer" : "Player Versus Player";

        ResetGame();
    }

    public void ToggleAIGoesFirst()
    {
        aiIsX = !aiIsX;

        aIgoesFirstText.text = aiIsX ?
            "Computer Goes First" : "Player Goes First";
        ResetGame();
    }

    public void SetDifficultyLevel()
    {
        Debug.Log("level " + difficultyLevelDropdown.value);
        aiDifficultyLevel = difficultyLevelDropdown.value;
        ResetGame();
    }

    /**
     * if aiIsX, starts the game with an X
     */
    private void Start()
    {
        //initialize openLocations
        for(int i = 0; i < 9; i++)
        {
            openLocations.Add(i);
        }

        if (aiIsX && usesAI) ChooseAIMove();
    }

    /**
     * Gets the location that was clicked
     * 
     * tries to place a mark there
     */
    public void ClickedLocation(int location)
    {
        //stops player from moving if not their turn and it is AI turn
        if (!gameActive || (usesAI && aiIsX && !isO)) return;

        //place mark
        PlaceMark(location);

        //tell AI to move
        if(usesAI) ChooseAIMove();
    }

    /**
     * Use Board marks to see if X or O are winners
     * 
     * 0,1,2
     * 3,4,5
     * 6,7,8
     * 
     * 0,3,6
     * 1,4,7
     * 2,5,8
     * 
     * 0,4,8
     * 2,4,6
     * 
     * if they do match and >0, 
     * Log X or O
     */
    public void CheckVictory()
    {
        for(int i = 0; i < 8; i++)
        {
            if (Check(boardMarks[checkNumbers[i,0]], boardMarks[checkNumbers[i, 1]], boardMarks[checkNumbers[i, 2]]))
            {
                gameActive = false;
                Debug.Log("Game Over: Winner is " + boardMarks[checkNumbers[i, 0]]);
                return;
            }
        }
    
        bool Check(int a, int b, int c)
        {
            return a > 0 && a == b && a == c;
        }
    }

    /**
     * moves for the AI
     */
    private void ChooseAIMove()
    {
        if (!gameActive || turn>8) return;
        //make easy first
        
        int location;

        switch (aiDifficultyLevel)
        {
            //easy
            case 0:
                //pick random available number
                location = GrabRandom();
                break;
            //medium
            case 1:
                //if there is a string of 2, place 1 at the end
                location = CheckStringOf2(true);
                break;
            //hard
            case 2:
                location = CheckStrategy();
                break;
            //easy
            default:
                //pick random available number
                location = GrabRandom();
                break;
        }

        //place at that number
        PlaceMark(location);

        /**
         * grabs a random available option
         */
        int GrabRandom()
        {
            return openLocations[Random.Range(0, openLocations.Count)];
        }

        /**
         * returns GrabRandom() if no strings
         * 
         * retuns location of string cap
         * 
         * Does not return strings with cap already
         */
        int CheckStringOf2(bool onNegRetGrabRandom)
        {
            int savedInt = -1;

            for(int i = 0; i < 8; i++)
            {
                int a= boardMarks[checkNumbers[i, 0]],
                    b= boardMarks[checkNumbers[i, 1]],
                    c= boardMarks[checkNumbers[i, 2]];

                bool checkedGroup = Check(a, b, c);

                if (checkedGroup)
                {
                    int clearNumb, markNumb;

                    if (a == 0)
                    {
                        clearNumb = 0;
                        markNumb = 1;
                    }
                    else
                    {
                        markNumb = 0;

                        if (b == 0)
                        {
                            clearNumb = 1;
                        }
                        else
                        {
                            clearNumb = 2;
                        }
                    }

                    if (XOR(aiIsX, boardMarks[checkNumbers[i, markNumb]] == 1))
                    {
                        return checkNumbers[i, clearNumb];
                    }
                    else
                    {
                        savedInt = checkNumbers[i, clearNumb];
                    }
                }
            }

            return savedInt>-1?savedInt:(onNegRetGrabRandom ? GrabRandom() : -1);

            bool Check(int a, int b, int c)
            {
                int clearCount = 0,
                    xCount=0,
                    oCount=0;

                foreach(int i in new int[]{a,b,c }){
                    switch (i)
                    {
                        case 0:
                            clearCount++;
                            //too many clear spaces
                            if (clearCount > 1) return false;
                            break;
                        case 1:
                            xCount++;
                            if (oCount > 0 || xCount > 2) return false;
                            break;
                        case 2:
                            oCount++;
                            if (xCount > 0 || oCount > 2) return false;
                            break;
                    }
                }

                return true;
            }

            bool XOR(bool aXOR, bool bXOR)
            {
                return (aXOR && bXOR) || (!(aXOR || bXOR));
            }
        }

        /**
         * Follows the strategy found in the game
         */
        int CheckStrategy()
        {
            return aiIsX?XStrategy():OStrategy();

            int XStrategy()
            {
                switch (turn)
                {
                    case 0:
                        return 4;
                    case 2:
                        #region case 2
                        //Did the other player play on a corner or a side?
                        for (int i = 0; i < 4; i++)
                        {
                            if (boardMarks[(i > 1 ? i + 1 : i) * 2] > 0)
                            {
                                switch (i)
                                {
                                    case 0:
                                        return 8;
                                    case 1:
                                        return 6;
                                    case 2:
                                        return 2;
                                    case 3:
                                        return 0;
                                }
                            }
                        }

                        //if we're still here, then they played on a side
                        //1,3,5,7

                        int[] choicesA = new int[] { 0, 2, 3, 5, 6, 8 },
                            choicesB = new int[] { 0,6,1,7,2,8 };

                        //find which side was played on
                        if (boardMarks[1] > 0)
                        {
                            //random play from {0, 2}, {3, 5}, {6, 8}
                            int choice = choicesA[Random.Range(0, choicesA.Length)];
                            return choice;
                        }
                        else if (boardMarks[3] > 0)
                        {
                            //random play from {0, 6}, {1, 7}, {2, 8}
                            int choice = choicesB[Random.Range(0, choicesB.Length)];
                            return choice;
                        }
                        else if (boardMarks[5] > 0)
                        {
                            //random play from {0, 6}, {1, 7}, {2, 8}
                            int choice = choicesB[Random.Range(0, choicesB.Length)];
                            return choice;
                        }
                        else if (boardMarks[7] > 0)
                        {
                            //random play from {0, 2}, {3, 5}, {6, 8}
                            int choice = choicesA[Random.Range(0, choicesA.Length)];
                            return choice;
                        }

                        //something went wrong
                        return CheckStringOf2(true);
                    #endregion case 2
                }

                int locToUse = CheckStringOf2(false);

                if (locToUse == -1) locToUse = TestFor3Trap();

                return locToUse;
            }

            int OStrategy()
            {
                switch (turn)
                {
                    case 1:
                        #region case 1
                        //x is in the middle place in a corner
                        if (boardMarks[4] > 0)
                        {
                            //grab a random corner
                            int[] possibleChoices = new int[] { 0, 2, 6, 8 };

                            oFirstTurn = possibleChoices[Random.Range(0, possibleChoices.Length)];

                            return oFirstTurn;
                        }
                        //x is in corner or on side, place in middle
                        return 4;
                        #endregion case 1
                    case 3:
                        #region case 3
                        //check string of 2
                        int getNumb = CheckStringOf2(false);

                        //x is middle
                        if (boardMarks[4] > 0)
                        {
                            //if get string of 2 is > -1 return that
                            if (getNumb > -1) return getNumb;

                            int cornerA = new int[] { 2, 6 }[Random.Range(0,2)], 
                                cornerB = new int[] { 0, 8 }[Random.Range(0, 2)];

                            //x placed across from us
                            switch (oFirstTurn)
                            {
                                case 0:
                                    return cornerA;
                                case 2:
                                    return cornerB;
                                case 6:
                                    return cornerB;
                                case 8:
                                    return cornerA;
                            }
                        }

                        //we are in the middle
                        if (getNumb > -1) return getNumb;

                        //x is in a line around us
                        if((boardMarks[0] == 1 && boardMarks[8] == 1) || (boardMarks[2] == 1 && boardMarks[6] == 1))
                        {
                            return new int[] { 1, 3, 5, 7 }[Random.Range(0, 4)];
                        }

                        return GrabRandom();
                        #endregion case 3
                }

                return CheckStringOf2(true);
            }
        }

        /**
         * CheckStringOf2(false) should be called first
         */
        int TestFor3Trap()
        {
            #region cross check && T check
            //4 is empty, then {1,3}, {3,7}, {7,5} or {5,1} are equal but nothing else
            int[,] numbsToCrossTCheck = new int[18, 5]
            {
                //cross 
                {1, 3, 5, 7, 4 },
                { 3,7,5,1,4 },

                //T
                { 0,2,4,7,1 },
                { 5,4,6,0,3 },
                { 1,4,6,8,7 },
                { 3,4,8,2,5 },

                //L
                { 0,1,5,8,2 },
                { 2,5,7,6,8 },
                { 8,7,3,0,6 },
                { 6,3,1,2,0 },

                //angle
                { 2,4,7,8,6 },
                { 0,4,7,6,8},
                { 0,4,5,2,8},
                { 6,4,5,8,2},
                { 6,4,1,0,2},
                { 8,4,1,2,0},
                { 8,4,3,6,0},
                { 2,4,0,3,6}
            };

            for(int i = 0; i < 18; i++)
            {
                if (CrossTCheck(boardMarks[numbsToCrossTCheck[i, 0]], 
                    boardMarks[numbsToCrossTCheck[i, 1]], 
                    boardMarks[numbsToCrossTCheck[i, 2]], 
                    boardMarks[numbsToCrossTCheck[i, 3]], 
                    boardMarks[numbsToCrossTCheck[i, 4]]))
                {
                    return numbsToCrossTCheck[i,4];
                }
            }
            #endregion cross check

            return GrabRandom();

            bool CrossTCheck(int a0, int a1, int b0, int b1, int middle)
            {
                //cound the numbs over 0
                int xCount = 0, oCount = 0;
                foreach (int i in new int[]{ a0,a1,b0,b1}){
                    if (i == 1)
                    {
                        if (oCount > 0 || !aiIsX) return false;
                        xCount++;
                    }
                    if (i == 2)
                    {
                        if (xCount > 0 || aiIsX) return false;
                        oCount++;
                    }
                }

                return (!(middle > 0 || (a0 > 0 && a1 > 0) || (b0 > 0 && b1 > 0))) && ((a0 > 0 || a1 > 0) && (b0 > 0 || b1 > 0));
            }
        }
    }

    private void PlaceMark(int location)
    {
        if (location < 0 || location > 8) throw new Exception("Location is invalid.");

        //verify location
        if (boardMarks[location] > 0)
        {
            return;
        }

        int assignInt = isO ? 2 : 1;

        boardMarks[location] = assignInt;

        Image image = boardLocations[location].GetComponent<Image>();

        if (image == null) throw new Exception("Image at " + location + " is null.");

        image.sprite = isO ? oSprite : xSprite;

        isO = !isO;

        CheckVictory();

        //remove location for AI
        openLocations.Remove(location);

        turn++;
    }
}
