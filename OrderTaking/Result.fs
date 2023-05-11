namespace global

// type Result<'Success, 'Failure> =
//     | Ok of 'Success
//     | Error of 'Failure

module Result =
    let bind f aResult =
        match aResult with
        | Ok success -> f success
        | Error failure -> Error failure


    let map f aResult =
        match aResult with
        | Ok success -> Ok(f success)
        | Error failure -> Error failure

    let mapError f aResult =
        match aResult with
        | Ok success -> Ok success
        | Error failure -> Error(f failure)
