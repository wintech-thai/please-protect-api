#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Configuration/org/#{orgId}/action/SetCurrentGeoLocation"
param = {
  ConfigValue: "Set this to JSON string of the geolocation you want to set. Example: {\"latitude\": 40.7128, \"longitude\": -74.0060}"
}

#result = make_request(:post, apiUrl, param)
#json = result.to_json
#puts(json)

apiUrl = "api/Configuration/org/#{orgId}/action/GetCurrentGeoLocation"
param = nil

result = make_request(:get, apiUrl, param)
json = result.to_json
puts(json)
