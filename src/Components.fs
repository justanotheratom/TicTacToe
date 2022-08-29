module Components

open Feliz
open Feliz.Router
open TicTacToe
open Fable.Auth0.React
open Fable.Core

type Page =
    | Login
    | StartGame
    | GameBoard
    | NotFound

let parseUrl = function
    | []                -> Page.Login
    | [ "startgame" ]   -> Page.StartGame
    | [ "gameboard"]    -> Page.GameBoard
    | _                 -> Page.NotFound

type Components () =

    [<Literal>]
    static let boardSize = 3

    [<ReactComponent>]
    static member LoginPage() =

        let ctxAuth0 = useAuth0 ()

        let login _ =
            let opts = unbox<RedirectLoginOptions> {| redirectUri = Browser.Dom.window.location.href + Router.format("startgame") |}
            ctxAuth0.loginWithRedirect opts
            |> Async.AwaitPromise
            |> Async.StartImmediate

        if ctxAuth0.isAuthenticated then
            Router.navigate("startgame")
            Html.none
        else
            Html.div [
                prop.style [ style.textAlign.center ]
                prop.children [
                    Html.h1 "Tic Tac Toe"
                    Html.button [
                        prop.text "LOGIN"
                        prop.onClick login
                    ]
                ]
            ]

    [<ReactComponent>]
    static member StartGamePage() =

        let ctxAuth0 = useAuth0 ()

        if ctxAuth0.isAuthenticated then
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
        else
            Router.navigate("login")
            Html.none

    [<ReactComponent>]
    static member GameBoardPage() =

        let ctxAuth0 = useAuth0 ()

        if ctxAuth0.isAuthenticated then

            let (gameState, setGameState) = React.useStateWithUpdater(TicTacToe.createGame boardSize)

            let resetGame () =
                setGameState (fun _ -> TicTacToe.createGame boardSize)

            let logout _ =
                resetGame ()
                let returnTo = Browser.Dom.window.location.origin
                let opts = unbox<LogoutOptions> {| returnTo = returnTo |}
                ctxAuth0.logout opts

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
                    Html.div [
                        Html.button [
                            prop.text "LOGOUT"
                            prop.onClick logout
                        ]
                    ]
                ]
            ]
        else
            Router.navigate("login")
            Html.none

    [<ReactComponent>]
    static member Router() =
        let (pageUrl, updateUrl) = React.useState(parseUrl(Router.currentUrl()))
        let currentPage =
            match pageUrl with
            | Page.Login     -> Components.LoginPage()
            | Page.StartGame -> Components.StartGamePage()
            | Page.GameBoard -> Components.GameBoardPage()
            | Page.NotFound  -> Html.h1 "Not Found"

        React.router [
            router.onUrlChanged (parseUrl >> updateUrl)
            router.children currentPage
        ]