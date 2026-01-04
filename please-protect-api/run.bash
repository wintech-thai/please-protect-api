#!/bin/bash

export $(grep -v '^#' .env | xargs)

dotnet run
