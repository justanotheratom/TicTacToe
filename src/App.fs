module App

open Components
open Browser.Dom
open Feliz
open Fable.Auth0.React

// JS equivalent: <Auth0Provider/>
let auth0App (children: seq<ReactElement>): ReactElement =
    let opts =
        unbox<Auth0ProviderOptions>
            {| domain = "dev-g54z9aby.us.auth0.com"
               clientId = "4yrGUE5TnJDuIc1uHWq8kiOwxjdNDevO"
               redirectUri = Browser.Dom.window.location.href |}
    Auth0Provider opts children

[<ReactComponent>]
let App () =
    React.fragment [ Components.Router() ]

ReactDOM.render (auth0App [App()],
                 document.getElementById "root")