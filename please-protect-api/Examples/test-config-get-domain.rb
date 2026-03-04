#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Configuration/org/#{orgId}/action/SetDomain"
param = {
  ConfigValue: "web-dev.rtarf-censor.dev-hubs.com"
}

#result = make_request(:post, apiUrl, param)
#json = result.to_json
#puts(json)

apiUrl = "api/Configuration/org/#{orgId}/action/GetDomain"
param = nil

result = make_request(:get, apiUrl, param)
json = result.to_json
puts(json)
