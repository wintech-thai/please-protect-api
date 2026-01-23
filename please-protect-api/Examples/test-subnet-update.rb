#!/usr/bin/env ruby

require 'net/http'
require 'uri'
require 'json'
require './utils'

$stdout.sync = true

################### Main #######################
load_env("./.env")

orgId = ENV['API_ORG']
id = '6a565d7c-472d-451c-9c27-128353ece516'

apiUrl = "api/Subnet/org/#{orgId}/action/UpdateSubnetById/#{id}"

param = {
  Cidr: "192.168.3.0/24",
  Name: "Home net internal #3",
  Tags: "internal",
}

result = make_request(:post, apiUrl, param)

json = result.to_json
puts(json)
