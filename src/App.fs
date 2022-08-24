module App

open Components
open Browser.Dom
open Fetch
open Thoth.Fetch
open Feliz


[<ReactComponent>]
let App () = React.fragment [ Components.TicTacToe() ]

ReactDOM.render (App(), document.getElementById "root")