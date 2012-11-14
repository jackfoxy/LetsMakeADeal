﻿(*Bayesian Monte Carlo of Let's Make a Deal
This code illustrates Bayes' Theorem in action on the Let's Make a Deal problem, which several authors 
have used to illustrate Bayes' Theorem. (It's easy to search the internet for further explanation.) 
Run with the audit option to audit up to the first 100 games. Running without audit is faster and can 
simulate a couple billion games.*)
//module game =
type game(doorWithPrize: int, fstChoice: int, revealedDoor: int) = 
    member x.winFstChoice = (doorWithPrize = fstChoice)
    member x.winSwitch = (not(doorWithPrize = fstChoice)&&(not(doorWithPrize = revealedDoor)))
    member x.revealed = revealedDoor
    member x.ToString = "Prize Door: " + doorWithPrize.ToString() + ";  " 
                            + "First Choice: " + fstChoice.ToString() + ";  " 
                            + "Revealed Door: " + revealedDoor.ToString() + ";  " 
                            + "Wins 1st Choice: " + x.winFstChoice.ToString() + ";  " 
                            + "Wins Switch: " + x.winSwitch.ToString()

module games =
    open System

    let seedRnd = new Random()

    let rndReveal doorWithPrize  =
        let rnd = seedRnd.Next(1,3)
        match doorWithPrize with
        |1 -> (rnd + 1)
        |2 -> if rnd = 1 then 1
              else 3  
        |_ -> rnd

    let forcedReveal doorWithPrize fstChoice = 
        match (doorWithPrize + fstChoice) with
        |3 -> 3 
        |4 -> 2  
        |_ -> 1

    ///<summary>Generate games and audit up to the first 100. Once the number of games gets into 10s of  
    ///millions the audit option becomesnoticeably slower.</summary>
    ///<param name="number">number of games to simulate</param>
    let gamesAudit number = 
        let rec recGames acc winsFst winsSwitch audit =
             let doorWithPrize = seedRnd.Next(1,4)
             let fstChoice =  seedRnd.Next(1,4)
             let revealedDoor =
                if (doorWithPrize = fstChoice) then rndReveal doorWithPrize
                else forcedReveal doorWithPrize fstChoice
             let myGame = game(doorWithPrize, fstChoice, revealedDoor)
             let newAudit =
                if (List.length audit) < 100 then (List.append audit [myGame])
                else audit
             if acc = 0 then (winsFst, winsSwitch), audit
             else if myGame.winFstChoice then recGames (acc - 1) (winsFst + 1) winsSwitch newAudit
                  else recGames (acc - 1) winsFst (winsSwitch + 1)  newAudit   
        let auditedGames = recGames number 0 0 List.Empty

        let rec writeAudit audit =
            match audit with
            |myGame:game::tl -> 
                Console.WriteLine(myGame.ToString)
                writeAudit tl
            |_ -> ignore

        let newAudit:List<game> = snd auditedGames
        writeAudit newAudit |> ignore
        if (fst(fst(auditedGames)) + snd(fst(auditedGames)) > 100) then Console.WriteLine("first 100 games audited")
        fst(auditedGames)

    ///<summary>Bare-bones generation of games. Once the number of games gets into 10s of millions this 
    ///is noticeably faster than generation with the audit option.</summary>
    ///<param name="number">number of games to simulate</param> 
    let games number = 
        let rec recGames acc winsFst winsSwitch =
             let doorWithPrize = seedRnd.Next(1,4)
             let fstChoice =  seedRnd.Next(1,4)
             (*The following source statement illustrates the essence of this problem as an example of  
             Bayes' Theorem. There are two posterior situations , in one case there are two possible  
             succeeding events, each with one-half probability. In the other case only one succeeding 
             event is possible with 100% probability.*)
             let revealedDoor =
                if (doorWithPrize = fstChoice) then rndReveal doorWithPrize
                else forcedReveal doorWithPrize fstChoice
             //strangely (on my 32-bit system), measuring large runs with StopWatch it is faster to  
             //instantiate myGame and query the member "winFstChoice" than do the direct 
             //"doorWithPrize = fstChoice" comparison 
             let myGame = game(doorWithPrize, fstChoice, revealedDoor)
             if acc = 0 then winsFst, winsSwitch
             else if myGame.winFstChoice then recGames (acc - 1) (winsFst + 1) winsSwitch 
                  else recGames (acc - 1) winsFst (winsSwitch + 1)     
        recGames number 0 0

    [<EntryPoint>]
    let main argv = 
         Console.WriteLine ("")
         Console.WriteLine ("P(A|B) = P(B|A)P(A) / P(B) //memorize this!")
         Console.WriteLine ("")

         if argv.Length  = 0 then Console.WriteLine ("Enter the number of games to simulate between 0 and 2147483647")
         else

            let myGames = 
                if argv.Length > 1 && argv.[1].ToLower() = "audit" then gamesAudit (Int32.Parse argv.[0])
                else games (Int32.Parse argv.[0])

            Console.WriteLine ((Int32.Parse argv.[0]).ToString("#,##0") + " games played")
            Console.WriteLine ("Wins with first choice: " + fst(myGames).ToString("#,##0") + ";  " 
                + "Wins switching: " + snd(myGames).ToString("#,##0"))
         
         Console.WriteLine ("")
         0 