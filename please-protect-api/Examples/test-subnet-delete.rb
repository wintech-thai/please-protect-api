#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = 'a8792fcb-6d2f-4ba2-adcc-3f51d3ff8170'

apiUrl = "api/Subnet/org/#{orgId}/action/DeleteSubnetById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
