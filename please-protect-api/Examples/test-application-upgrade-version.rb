#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Application/org/#{orgId}/action/VersionUpgrade"
param = {
  "FromVersion" => "v1.0.0",
  "ToVersion" => "v2.0.0"
}
result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
