#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
appName = 'app-censor-arkime'

apiUrl = "api/Application/org/#{orgId}/action/GetCurrentAppCustomConfig/#{appName}"
param = nil
result = make_request(:get, apiUrl, param)

#json = result.to_json
puts(result)
