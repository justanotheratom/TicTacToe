module App

open Components
open Browser.Dom
open Feliz

[<ReactComponent>]
let App () =
    React.fragment [ Components.Router() ]

ReactDOM.render (App(),
                 document.getElementById "root")