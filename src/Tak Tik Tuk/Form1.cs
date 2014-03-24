/**
 * Tak Tik Tuk Game_Based on a Neural Computing Algorithm
 * Identifying patterns and analyzing them to make decisions
 * Copyrights © 2012. All rights reserved
 * Author Amith Chinthaka <amithchinthaka@ieee.org>
 * 
 * Tak Tik Tuk Board Game is free software; you can redistribute it
 * and/or modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2 of
 * the License, or (at your option) any later version.
 * 
 * Language : Visual C#.NET 3.5 Framework (Ms VS 2008)
 * Available on Oct 27th 2012
 * 
 * NOTE : Please don't remove this sentence in About dialog
 * "It's your gift... SMiLE"
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Tak_Tik_Tuk
{
    public partial class Form1 : Form
    {
        //turn=false : player's turn
        private bool turn;
        private int times = 0;

        //Store computer's steps and no. of steps
        private int[] computer = { 31, 31, 31, 31, 31 };
        private int ittComputer = 0;
        private bool playStatus = true;
        private bool decisionStatus = false;
        //Store player's steps and no. of steps
        private int[] player = { 31, 31, 31, 31, 31 };
        private int ittPlayer = 0;

        //Store mark: cross or oval
        private Image mark;
        private Image[] marks = new Image[2];

        /* Store game steps taken by each player and meta data
         * record[0] - win/lose; win=0, lose=1
         * record[1:9] - steps
         * record[10] - first played; computer=1, player=2
         * record[11] - computer won; won=0, drawn=1, lose=2
         */ 
        private int[] record = new int[12];

        /* Data structures which patterns stored
         * 010 - win + first played:computer + won:computer
         * 022 - win + first played:player + won:player
         * 
         * 012 - win + first played:computer + won:player
         * 020 - win + first played:player + won:computer
         * 
         * 101 - Draw
         */
        List<String> ptn010 = new List<String>();
        List<String> ptn012 = new List<String>();
        List<String> ptn020 = new List<String>();
        List<String> ptn022 = new List<String>();
        List<String> ptn101 = new List<String>();

        List<String> tempList = new List<String>();
        List<int> free = new List<int>();

        //Selecting record at first time
        private Random getRecSet = new Random();
        //true-010 : false-022 for first click
        private bool recSet = true;
        
        //File handling
        private TextWriter tw;
        private TextReader tr;
        private bool writeStatus = false;

        private void checkDuplicate(String pattern, List<String> lst)
        {
            foreach (String ptn in lst)
            {
                if (ptn.Equals(pattern) | writeStatus == false)
                {
                    writeStatus = false;
                    break;
                }
            }
        }

        private void writeRecords()
        {
            String pattern = record[1] + "" + record[2] + "" + record[3] + "" + record[4]
                + "" + record[5] + "" + record[6] + "" + record[7] + "" + record[8] + "" + record[9];

            //Avoiding duplications
            if (record[10] == 1)
            {
                checkDuplicate(pattern, ptn012);
                checkDuplicate(pattern, ptn020);
            }
            else
            {
                checkDuplicate(pattern, ptn010);
                checkDuplicate(pattern, ptn022);
            }

            if(writeStatus == true)
            {
                //writing data into file
                try
                {
                    tw.WriteLine(record[0] + "" + pattern + "" + record[10] + "" + record[11]);
                    tw.Flush();
                }
                catch (Exception e)
                {
                    MessageBox.Show("" + e);
                }

                addIntoLists(record);
            }
        }

        //Add new patterns into data structures
        private void addIntoLists(int[] Arr)
        {
            //inserting data into List
            int ptn = Arr[10] + Arr[11];
            String pattern = Arr[1] + "" + Arr[2] + "" + Arr[3] + "" + Arr[4] + "" +
                    Arr[5] + "" + Arr[6] + "" + Arr[7] + "" + Arr[8] + "" + Arr[9];
            if (Arr[0] == 1)
            {
                //101
                ptn101.Add(pattern);
            }
            else if(ptn == 1)
            {
                //010
                ptn010.Add(pattern);
            }
            else if (ptn == 2)
            {
                //020
                ptn020.Add(pattern);
            }
            else if (ptn == 3)
            {
                //012
                ptn012.Add(pattern);
            }
            else if (ptn == 4)
            {
                //022
                ptn022.Add(pattern);
            }
        }

        //******************************GAME ALGORITHMS*************************************

        /* Core of the supportive Algorithm
         * Based on combnation theory
         * return next click
         * start, end, series - From combination algorithm
         * arr:The array which going to store combinations
         * cnt:No. of items in arr
         * check:Check for risky patterns
         */
        private int combinations(int start, int end, int series, int r, int[] arr, int cnt, bool check)
        {
            int n = -1;

            for (int i = start; i <= end; i++)
            {
                arr[cnt + series - 1] = free[i];

                if (series < r)
                {
                    n = combinations(i + 1, end + 1, series + 1, r, arr, cnt, check);
                }
                else
                {
                    if(checkWin(arr) == true)
                    {
                        bool matched = false;

                        if (check == true)
                        {
                            foreach (String ptn in tempList)
                            {
                                if (Int32.Parse("" + ptn[times]) == arr[cnt])
                                {
                                    matched = true;
                                    break;
                                }
                            }                            
                        }

                        if (matched == false)
                        {
                            n = arr[cnt];
                            return n;
                        }
                    }
                }

                if (n > 0)
                {
                    return n;
                }
            }

            return n;
        }

        /* Supportive Algorithm for create new patterns
         * return next click
         */
        private int makeDecision()
        {
            int n = -1;
            int[] arr = new int[5];

            //Try to win in next click
            if (ittComputer >= 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    arr[i] = computer[i];
                }
                if (ittComputer >= 2)
                {
                    n = combinations(0, (free.Count - 1), 1, 1, arr, ittComputer, false);
                    if (n != -1)
                    {
                        //MessageBox.Show("Win " + n);
                        return n;
                    }
                }
            }

            //Avoiding human player's win
            if (ittPlayer >= 2)
            {
                for (int i = 0; i < 5; i++)
                {
                    arr[i] = player[i];
                }
                n = combinations(0, (free.Count - 1), 1, 1, arr, ittPlayer, false);
                if (n != -1)
                {
                    //MessageBox.Show("Avoid " + n);
                    return n;
                }
            }

            int balance = 0;
            if (record[10] == 1)
            {
                balance = 0;
            }
            else
            {
                balance = -1;
            }

            //If there is no risk, try to win or draw
            if (ittComputer == 4)
            {
                n = free[0];
                //MessageBox.Show("Last " + n);
                return n;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    arr[i] = computer[i];
                }
                int r = 5 - ittComputer + balance;
                n = combinations(0, (free.Count - r), 1, r, arr, ittComputer, true);
                if (n != -1)
                {
                    //MessageBox.Show("To win " + n);
                    return n;
                }
                
            }

            //Select click randomly as last option
            if (n < 0)
            {
                n = getRecSet.Next(1, 9);
                for (int i = 1; i <= times; i++)
                {
                    if (record[i] == n)
                    {
                        n = getRecSet.Next(1, 9);
                        i = 0;
                    }
                }
                //MessageBox.Show("random " + n);
                return n;
            }

            return n;
        }

        /* Select records from list lst according to pattern
         * And add matched sets to tempList
         */
        private void selectRecords(List<String> lst)
        {
            tempList.Clear();
            foreach (String temp in lst)
            {
                int mached = 0;
                for (int i = 0; i < times; i++)
                {
                    if (record[i + 1] == Int32.Parse("" + temp[i]))
                    {
                        mached++;
                    }
                }
                if(mached == times)
                {
                    tempList.Add(temp);
                }
            }
        }

        /* Computer Game Algorithm: Based on AI
         * Returns the next click
         */
        private int computerNext()
        {
            int n = 0;
            int setNo = 0;

            if(times == 0)
            {
                /* First Click
                 * Randomly choosen from either ptn010 or ptn022
                 * One list has been selected by one after one each time
                 */
                if(recSet == true)
                {
                    setNo = getRecSet.Next(ptn010.Count - 1);
                    n = Int32.Parse("" + ptn010[setNo][1]);
                    recSet = false;
                }
                else
                {
                    setNo = getRecSet.Next(ptn022.Count - 1);
                    n = Int32.Parse("" + ptn022[setNo][1]);
                    recSet = true;
                }

            }
            else if(record[10] == 1 && decisionStatus == false)
            {
                /* Computer first played
                 * Select one record set among matching records
                 * First search in ptn010
                 * Second in ptn022
                 * Last in ptn101
                 * if not found, choose next click by (...)
                 */

                selectRecords(ptn010);
                if (tempList.Count == 0)
                {
                    selectRecords(ptn022);
                    if (tempList.Count == 0)
                    {
                        selectRecords(ptn101);
                        if (tempList.Count == 0)
                        {
                            //Chances of lose
                            selectRecords(ptn012);
                            if (tempList.Count == 0)
                            {
                                selectRecords(ptn020);
                            }

                            //Supportive Algorithm for create new patterns
                            n = makeDecision();
                            decisionStatus = true;
                            writeStatus = true;
                            return n;
                        }                        
                    }                    
                }                
                setNo = getRecSet.Next(tempList.Count - 1);
                n = Int32.Parse("" + tempList[setNo][times]);
            }
            else if (record[10] == 2 && decisionStatus == false)
            {
                /* Player first played
                 * Select one record set among matching records
                 * First search in ptn020
                 * Second in ptn012
                 * Last in ptn101
                 * if not found, choose next click by supportive algorithm
                 */

                selectRecords(ptn020);
                if (tempList.Count == 0)
                {
                    selectRecords(ptn012);
                    if (tempList.Count == 0)
                    {
                        selectRecords(ptn101);
                        if (tempList.Count == 0)
                        {
                            //Chances of lose
                            selectRecords(ptn010);
                            if (tempList.Count == 0)
                            {
                                selectRecords(ptn022);
                            }

                            //Supportive Algorithm for create new patterns
                            n = makeDecision();
                            decisionStatus = true;
                            writeStatus = true;                            
                            return n;
                        }
                    }
                }
                setNo = getRecSet.Next(tempList.Count - 1);
                n = Int32.Parse("" + tempList[setNo][times]);
            }
            else
            {
                n = makeDecision();
                return n;
            }

            return n;
        }

        /* Computer Player
         * Do exactly same thing in button click event
         */
        private void computerPlayer()
        {
            if(playStatus == true)
            {
                //Set some delay
                System.Threading.Thread.Sleep(500);

                //Functionality of a player
                int nextPosition = computerNext();
                if (nextPosition > 0)
                {
                    setMark(nextPosition);
                    switch (nextPosition)
                    {
                        case 4:
                            box1.Image = mark;
                            box1.Enabled = false;
                            break;
                        case 9:
                            box2.Image = mark;
                            box2.Enabled = false;
                            break;
                        case 2:
                            box3.Image = mark;
                            box3.Enabled = false;
                            break;
                        case 3:
                            box4.Image = mark;
                            box4.Enabled = false;
                            break;
                        case 5:
                            box5.Image = mark;
                            box5.Enabled = false;
                            break;
                        case 7:
                            box6.Image = mark;
                            box6.Enabled = false;
                            break;
                        case 8:
                            box7.Image = mark;
                            box7.Enabled = false;
                            break;
                        case 1:
                            box8.Image = mark;
                            box8.Enabled = false;
                            break;
                        case 6:
                            box9.Image = mark;
                            box9.Enabled = false;
                            break;
                    }
                }
            }
        }

        //**********************************************************************************

        private bool checkWin(int[] arr)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    for (int k = j + 1; k < 5; k++)
                    {
                        int sum = arr[i] + arr[j] + arr[k];
                        if (sum == 15)
                        {
                            record[0] = 0;
                            return true;
                        }
                    }
                }
            }
            if (times == 9)
            {
                record[0] = 1;
                record[11] = 1;
                if (writeStatus == true)
                {
                    writeRecords();
                }
                MessageBox.Show("Game Drawn... Please try again");
                unlockTable(false);
            }
            return false;
        }

        private void setMark(int n)
        {
            times++;
            record[times] = n;
            free.Remove(n);
            if (turn == true)
            {
                turn = false;
                computer[ittComputer] = n;
                if (ittComputer >= 2 && checkWin(computer) == true)
                {
                    record[11] = 0;
                    if (writeStatus == true)
                    {
                        writeRecords();
                    }
                    MessageBox.Show("Try Again, Computer Won...");
                    lblComputerMarks.Text = "" + (Int32.Parse(lblComputerMarks.Text) + 1);
                    unlockTable(false);
                }
                else
                {
                    ittComputer++;
                }
                
                mark = marks[1];
            }
            else
            {
                turn = true;
                player[ittPlayer] = n;
                if (ittPlayer >= 2 && checkWin(player) == true)
                {
                    record[11] = 2;
                    if (writeStatus == true)
                    {
                        writeRecords();
                    }
                    MessageBox.Show("Congratulations !!! You won :D");
                    lblPlayerMarks.Text = "" + (Int32.Parse(lblPlayerMarks.Text) + 1);
                    unlockTable(false);
                }
                else
                {
                    ittPlayer++;
                }
                
                mark = marks[0];
            }
        }

        private void unlockTable(bool status)
        {
            box1.Enabled = status;
            box2.Enabled = status;
            box3.Enabled = status;
            box4.Enabled = status;
            box5.Enabled = status;
            box6.Enabled = status;
            box7.Enabled = status;
            box8.Enabled = status;
            box9.Enabled = status;
            playStatus = status;
        }

        private void reset()
        {
            mark = null;
            box1.Image = mark;
            box2.Image = mark;
            box3.Image = mark;
            box4.Image = mark;
            box5.Image = mark;
            box6.Image = mark;
            box7.Image = mark;
            box8.Image = mark;
            box9.Image = mark;
            unlockTable(true);

            ittComputer = 0;
            ittPlayer = 0;
            times = 0;
            decisionStatus = false;

            for (int i = 0; i < 5; i++)
            {
                computer[i] = 31;
                player[i] = 31;
            }
            int firstPlayed = record[10];
            for (int i = 0; i < 12; i++)
            {
                record[i] = 0;
            }
            free.Clear();
            for (int i = 1; i < 10; i++ )
            {
                free.Add(i);
            }

            if (firstPlayed == 2)
            {
                turn = true;
                record[10] = 1;
                computerPlayer();
            }
            else
            {
                turn = false;
                record[10] = 2;
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void box1_Click(object sender, EventArgs e)
        {
            setMark(4);
            box1.Image = mark;
            box1.Enabled = false;
            computerPlayer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //Reading record.data file and load records into Lists
                tr = new StreamReader("../../data/records.data");
                while(tr.Peek() != -1 )
                {
                    String temp = tr.ReadLine();
                    int[] tempRec = new int[12];

                    for (int i = 0; i < 12; i++)
                    {
                        tempRec[i] = Int32.Parse("" + temp[i]);
                    }

                    addIntoLists(tempRec);
                }
                tr.Close();

                //Prepare record.data to write new records
                tw = new StreamWriter("records.data", true, UTF8Encoding.ASCII, 12);
            }
            catch (Exception err)
            {
                MessageBox.Show("" + err);
            }

            turn = false;
            record[10] = 1;
            marks[0] = Image.FromFile("../../data/marks/mark1.png");
            marks[1] = Image.FromFile("../../data/marks/mark2.png");
            reset();
            toolTip1.SetToolTip(picReset, "New Game");
        }

        private void box2_Click(object sender, EventArgs e)
        {
            setMark(9);
            box2.Image = mark;
            box2.Enabled = false;
            computerPlayer();
        }

        private void box3_Click(object sender, EventArgs e)
        {
            setMark(2);
            box3.Image = mark;
            box3.Enabled = false;
            computerPlayer();
        }

        private void box4_Click(object sender, EventArgs e)
        {
            setMark(3);
            box4.Image = mark;
            box4.Enabled = false;
            computerPlayer();
        }

        private void box5_Click(object sender, EventArgs e)
        {
            setMark(5);
            box5.Image = mark;
            box5.Enabled = false;
            computerPlayer();
        }

        private void box6_Click(object sender, EventArgs e)
        {
            setMark(7);
            box6.Image = mark;
            box6.Enabled = false;
            computerPlayer();
        }

        private void box7_Click(object sender, EventArgs e)
        {
            setMark(8);
            box7.Image = mark;
            box7.Enabled = false;
            computerPlayer();
        }

        private void box8_Click(object sender, EventArgs e)
        {
            setMark(1);
            box8.Image = mark;
            box8.Enabled = false;
            computerPlayer();
        }

        private void box9_Click(object sender, EventArgs e)
        {
            setMark(6);
            box9.Image = mark;
            box9.Enabled = false;
            computerPlayer();
        }

        private void picReset_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void lblInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form2 f2 = new Form2();
            f2.ShowDialog();
        }

    }
}
