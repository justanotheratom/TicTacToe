module Components

open Fable.Core
open Fable.SignalR
open Fable.SignalR.Feliz
open Feliz
open Feliz.Router
open Fetch
open Thoth.Fetch

open TicTacToe

type Page =
    | StartGame
    | GameBoard
    | NotFound

let parseUrl = function
    | []                -> Page.StartGame
    | [ "startgame" ]   -> Page.StartGame
    | [ "gameboard"]    -> Page.GameBoard
    | _                 -> Page.NotFound

module SignalRHub =
    [<RequireQualifiedAccess>]
    type Action =
        | UpdateGameState

    [<RequireQualifiedAccess>]
    type Response =
        | UpdateGameState

type Components () =

    [<Literal>]
    static let boardSize = 3

    [<ReactComponent>]
    static member Message () =
        let (message, setMessage) = React.useState ("")

        Html.div [
            prop.children [
                Html.button [
                    prop.text "Get a message from the API"
                    prop.onClick(
                        fun _ ->
                            promise {
                                let! message =
                                    Fetch.get (
                                        "/api/GetMessage?name=FSharp",
                                        headers = [ HttpRequestHeaders.Accept "application/json" ]
                                    )

                                setMessage message
                                return ()
                            }
                            |> ignore
                    )
                ]
                if message = "" then
                    Html.none
                else
                    Html.p message
            ]
        ]

    [<ReactComponent>]
    static member StartGamePage() =
            Html.div [
                prop.style [ style.textAlign.center ]
                prop.children [
                    Html.h1 "Tic Tac Toe"
                    Html.button [
                        prop.text "START GAME"
                        prop.onClick (fun _ -> Router.navigate("gameboard"))
                    ]
                ]
            ]

    [<ReactComponent>]
    static member GameBoardPage() =

        let (gameState, setGameState) = React.useStateWithUpdater(TicTacToe.createGame boardSize)

        let resetGame () =
            setGameState (fun _ -> TicTacToe.createGame boardSize)

        Html.div [
            prop.style [ style.textAlign.center ]
            prop.children [
                Html.h1 "Tic Tac Toe"
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
                        prop.onClick (fun _ ->
                            resetGame ()
                            Router.navigate("startgame")
                        )
                    ]
                    Html.button [
                        prop.text "Start a fresh game"
                        prop.onClick (fun _ -> resetGame ())
                    ]
                ]
            ]
        ]

    [<ReactComponent>]
    static member Router() =

        let hub =
            React.useSignalR<SignalRHub.Action,SignalRHub.Response>(
                fun hub -> 
                    hub.withUrl("/api")
                        .withAutomaticReconnect()
                        .configureLogging(LogLevel.Debug)
                        .onMessage <|
                            function
                            | SignalRHub.Response.UpdateGameState -> ()
            )

        let (pageUrl, updateUrl) = React.useState(parseUrl(Router.currentUrl()))

        let currentPage =
            match pageUrl with
            | Page.StartGame -> Components.StartGamePage()
            | Page.GameBoard -> Components.GameBoardPage()
            | Page.NotFound  -> Html.h1 "Not Found"

        React.router [
            router.onUrlChanged (parseUrl >> updateUrl)
            router.children [
                currentPage
            ]
        ]