namespace Components

open Feliz
open Fable.Core.JS

type Components () =

    [<Literal>]
    static let boardSize = 3

    [<ReactComponent>]
    static member TicTacToe() =

        let (gameStarted, setGameStarted) = React.useState(false)

        let resetGameState () =
            ()

        Html.div [
            prop.style [ style.textAlign.center ]
            prop.children [
                Html.h1 "Tic Tac Toe"
                if not gameStarted then
                    Html.button [
                        prop.text "Start game"
                        prop.onClick (fun _ -> setGameStarted true)
                    ]
                else
                    Html.p [
                        Html.text (sprintf "Player %s moves" "X")
                    ]
                    Html.div [
                        prop.style [
                            style.margin.auto
                            style.marginBottom 10
                            style.backgroundColor.black
                            style.display.grid
                            style.gridTemplateColumns (boardSize, length.px 100)
                            style.gridTemplateRows (boardSize, length.px 100)
                            style.columnGap (length.px 4)
                            style.rowGap (length.px 4)
                            style.width (length.px 308)
                        ]
                        prop.children [
                            for y in 0..(boardSize*boardSize-1) do
                                Html.div [
                                    prop.style [
                                        style.backgroundColor.white
                                        style.fontSize (length.px 90)
                                    ]
                                    prop.text "O"
                                ]
                        ]
                    ]
                    Html.div [
                        Html.button [
                            prop.text "Abandon game"
                            prop.onClick (
                                fun _ ->
                                    setGameStarted false
                                    resetGameState ()
                            )
                        ]
                        Html.button [
                            prop.text "Start a fresh game"
                            prop.onClick (
                                fun _ ->
                                    setGameStarted false
                                    resetGameState ()
                                    setGameStarted true
                            )
                        ]
                    ]
            ]
        ]