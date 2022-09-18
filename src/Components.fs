module Components

open Fable.Core
open Fable.SignalR
open Fable.SignalR.Feliz
open Feliz
open Feliz.Router
open Fetch
open Thoth.Fetch

open TicTacToe

let [<Global>] console: JS.Console = jsNative

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
        | CreateGameRoom of string
        | JoinGameRoom of string
        | UpdateGameState of TicTacToe.GameState

    [<RequireQualifiedAccess>]
    type Response =
        | GameRoomCreated of string
        | UpdateGameState of TicTacToe.GameState

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
    static member StartGamePage (hub: IRefValue<Fable.SignalR.Hub<SignalRHub.Action,SignalRHub.Response>>) =

        let (playerName, setPlayerName) = React.useState ("")
        let (gameRoomName, setGameRoomName) = React.useState ("")

        Html.div [
            prop.style [ style.textAlign.center ]
            prop.children [
                Html.h1 "Tic Tac Toe"
                Html.div [
                    prop.children [
                        Html.label [
                            prop.text "Name"
                        ]
                        Html.input [
                            prop.style [
                                style.margin 10
                            ]
                            prop.type' "text"
                            prop.placeholder "Your Name"
                            prop.onTextChange (fun name -> (setPlayerName name))
                        ]
                    ]
                ]
                Html.div [
                    prop.children [
                        Html.label [
                            prop.text "Game Room"
                        ]
                        Html.input [
                            prop.style [
                                style.margin 10
                            ]
                            prop.type' "text"
                            prop.placeholder "Game Room Name"
                            prop.onTextChange (fun name -> (setGameRoomName name))
                        ]
                    ]
                ]
                Html.button [
                    prop.text "CREATE"
                    prop.onClick <| fun _ -> 
                        promise {
                            console.log("invoking CreateGameRoom")
                            let! rsp = hub.current.invokeAsPromise (SignalRHub.Action.CreateGameRoom gameRoomName)

                            match rsp with
                            | SignalRHub.Response.GameRoomCreated gameRoomName ->
                                console.log(gameRoomName)
                                Router.navigate("gameboard")
                            | _ -> ()
                        }
                        |> Promise.start
                ]
                Html.button [
                    prop.text "JOIN"
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
                            | SignalRHub.Response.GameRoomCreated roomName -> ()
                            | SignalRHub.Response.UpdateGameState gameState -> ()
            )

        let (pageUrl, updateUrl) = React.useState(parseUrl(Router.currentUrl()))

        let currentPage =
            match pageUrl with
            | Page.StartGame -> Components.StartGamePage hub
            | Page.GameBoard -> Components.GameBoardPage()
            | Page.NotFound  -> Html.h1 "Not Found"

        React.router [
            router.onUrlChanged (parseUrl >> updateUrl)
            router.children [
                currentPage
            ]
        ]