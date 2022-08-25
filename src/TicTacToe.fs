module TicTacToe

type Player =
    | X
    | O

type Cell =
    | Empty
    | Taken of Player

type GameStatus =
    | NotStarted
    | Started
    | Decided of Player * int list

type GameState =
    {
        BoardSize: int
        Status: GameStatus
        NextTurn: Player
        Grid: Cell list
    }

let toIndex gameState x y =
    y * gameState.BoardSize + x

let createGame boardSize =
    {
        BoardSize = boardSize
        Status = NotStarted
        NextTurn = X
        Grid = [
            for y in 0..boardSize-1 do
                for x in 0..boardSize-1 do
                    Empty
        ]
    }

let started gameState =
    gameState.Status <> NotStarted

let decided gameState =
    match gameState.Status with
    | Decided _ -> true
    | _ -> false

let winner gameState =
    match gameState.Status with
    | Decided (p, _) -> p
    | _ -> O

let isWinningCell gameState x y =
    match gameState.Status with
    | (Decided (p, winningSeries)) -> winningSeries |> List.contains (toIndex gameState x y)
    | _ -> false

let nextTurn gameState =
    gameState.NextTurn

let startGame gameState =
    {
        gameState with
            Status = Started
    }

let decide gameState =

    let winningSeries =
        [
            for x in 0..(gameState.BoardSize-1) do
                [ for y in 0..(gameState.BoardSize-1) do (x, y)]

            for y in 0..(gameState.BoardSize-1) do
                [ for x in 0..(gameState.BoardSize-1) do (x, y)]

            [ 0..(gameState.BoardSize-1) ] |> List.zip [ 0..(gameState.BoardSize-1) ]

            [ (gameState.BoardSize-1) .. -1 .. 0 ] |> List.zip [ 0..(gameState.BoardSize-1) ]
        ]
        |> List.filter (
            fun series ->
                series
                |> List.pairwise
                |> List.forall (
                    fun ((x1, y1), (x2, y2)) ->
                        gameState.Grid.[toIndex gameState x1 y1] <> Empty
                        &&
                        gameState.Grid.[toIndex gameState x1 y1] = gameState.Grid.[toIndex gameState x2 y2]
                )
        )
        |> List.map (
            fun series ->
                series
                |> List.map (fun (x, y) -> toIndex gameState x y)
        )

    if (List.isEmpty winningSeries) then
        gameState
    else
        let winner =
            match gameState.NextTurn with
            | X -> O
            | O -> X

        {
            gameState with
                Status = Decided (winner, winningSeries.[0])
        }

let takeCell gameState x y =

    if decided gameState then
        gameState
    else
        match gameState.Grid.[toIndex gameState x y] with
        | Empty ->
            decide {
                gameState with
                    NextTurn = match gameState.NextTurn with
                               | X -> O
                               | O -> X
                    Grid = gameState.Grid |> List.updateAt (toIndex gameState x y) (Taken gameState.NextTurn)
            }
        | Taken p -> gameState

let getCell gameState x y =

    gameState.Grid.[toIndex gameState x y]