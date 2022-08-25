namespace Components

open Feliz
open Fable.Core.JS
open TicTacToe

type Components () =

    [<Literal>]
    static let boardSize = 3

    [<ReactComponent>]
    static member TicTacToe() =

        let (gameState, setGameState) = React.useStateWithUpdater(TicTacToe.createGame boardSize)

        Html.div [
            prop.style [ style.textAlign.center ]
            prop.children [
                Html.h1 "Tic Tac Toe"
                if not (TicTacToe.started gameState) then
                    Html.button [
                        prop.text "Start game"
                        prop.onClick (fun _ -> setGameState (fun prevState -> TicTacToe.startGame prevState))
                    ]
                else
                    Html.p [
                        Html.text (
                            if (TicTacToe.decided gameState) then
                                sprintf "Player %s wins!!"
                                        (
                                            match (TicTacToe.winner gameState) with
                                            | X -> "X"
                                            | O -> "O"
                                        )
                            else
                                sprintf "Player %s moves"
                                        (
                                            match (TicTacToe.nextTurn gameState) with
                                            | X -> "X"
                                            | O -> "O"
                                        )
                        )
                    ]
                    Html.div [
                        prop.style [
                            style.margin.auto
                            style.marginBottom 20
                            style.backgroundColor.black
                            style.display.grid
                            style.gridTemplateColumns (boardSize, length.px 100)
                            style.gridTemplateRows (boardSize, length.px 100)
                            style.columnGap (length.px 4)
                            style.rowGap (length.px 4)
                            style.width (length.px ((100 * boardSize) + (4 * (boardSize - 1))))
                        ]
                        prop.children [
                            for i in 0..(boardSize-1) do
                                for j in 0..(boardSize-1) do
                                    Html.div [
                                        prop.style [
                                            style.fontSize (length.px 90)
                                            if ((TicTacToe.decided gameState)
                                                &&
                                                (TicTacToe.isWinningCell gameState i j)) then
                                                style.backgroundColor.cyan
                                            else
                                                style.backgroundColor.white
                                        ]
                                        prop.onClick (
                                            fun _ -> setGameState (fun prevState -> TicTacToe.takeCell prevState i j)
                                        )
                                        prop.text (
                                            match (TicTacToe.getCell gameState i j) with
                                            | Empty -> ""
                                            | Taken X -> "X"
                                            | Taken O -> "O"
                                        )
                                    ]
                        ]
                    ]
                    Html.div [
                        Html.button [
                            prop.text "Abandon game"
                            prop.onClick (
                                fun _ ->
                                    setGameState (fun _ -> TicTacToe.createGame boardSize)
                            )
                        ]
                        Html.button [
                            prop.text "Start a fresh game"
                            prop.onClick (
                                fun _ ->
                                    setGameState (fun _ -> TicTacToe.createGame boardSize |> TicTacToe.startGame)
                            )
                        ]
                    ]
            ]
        ]