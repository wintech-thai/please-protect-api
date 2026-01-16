#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = '03671174-c1e3-4c8f-bcd2-a103acad3559'

apiUrl = "api/Document/org/#{orgId}/action/DeleteDocumentById/#{id}"
param = nil

result = make_request(:delete, apiUrl, param)

json = result.to_json
puts(json)
