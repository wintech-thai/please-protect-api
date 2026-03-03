#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']

apiUrl = "api/Es/org/#{orgId}/action/GetIndices"
param = {
  "Offset": 1,
  "Limit": 5
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
