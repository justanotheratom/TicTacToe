module Api.Negotiate

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.Extensions.Logging
open Microsoft.Azure.WebJobs.Extensions.SignalRService

[<FunctionName("negotiate")>]
let negotiate
    ([<HttpTrigger(AuthorizationLevel.Anonymous)>] req: HttpRequest)
    ([<SignalRConnectionInfo (HubName = "tictactoe")>] connectionInfo: SignalRConnectionInfo)
    :
    SignalRConnectionInfo
    =
    connectionInfo
